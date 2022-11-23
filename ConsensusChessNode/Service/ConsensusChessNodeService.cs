using System;
using System.Collections;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ConsensusChessNode.Service
{
    public class ConsensusChessNodeService : AbstractConsensusService
    {
        // TODO: be a good citizen - set a polling period that isn't too disruptive
        private static readonly TimeSpan DefaultPollPeriod = TimeSpan.FromSeconds(30);
        private TimeSpan? overridePollPeriod;
        protected override TimeSpan PollPeriod => overridePollPeriod ?? DefaultPollPeriod;
        protected override NodeType NodeType => NodeType.Node;

        public ConsensusChessNodeService(ILogger log, ServiceIdentity id, DbOperator dbo, Network network, ISocialConnection social, TimeSpan? overridePollPeriod = null)
            : base(log, id, dbo, network, social)
        {
            this.overridePollPeriod = overridePollPeriod;
        }

        protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            using (var db = dbo.GetDb())
            {
                // find and post uposted boards from active games
                var unpostedBoardChecks = gm.FindUnpostedActiveBoards(db.Games, state.Shortcode);
                foreach (var check in unpostedBoardChecks)
                {
                    var game = check.Key;
                    var board = check.Value;
                    if (board != null)
                    {
                        log.LogInformation($"Found a new board to post in game: {game.Id}");

                        var post = new PostBuilder(PostType.Node_BoardUpdate)
                            .WithGame(game)
                            .WithBoard(board)
                            .Build();
                        var posted = await social.PostAsync(post);
                        board.BoardPosts.Add(posted);

                        log.LogDebug("Saving board and new board posts...");
                        await db.SaveChangesAsync();
                    }
                }

                // TODO: find and post unposted abandoned games

                // TODO: find and post unposted ended games

            }
        }

        protected override void RegisterForCommands(CommandProcessor processor)
        {
            processor.Register("shutdown", requireAuthorised: true, runsRetrospectively: false, ShutdownAsync);
            processor.Register("move", requireAuthorised: false, runsRetrospectively: true, ProcessVoteAsync);
        }

        private async Task ProcessVoteAsync(SocialCommand origin, IEnumerable<string> words)
        {
            log.LogDebug($"Processing vote from {origin.SourceUsername.Full}, in reply to {origin.InReplyToId?.ToString() ?? "(none)"}: {string.Join(" ", words)}");

            var voteSAN = string.Join(" ", words.Skip(1));
            log.LogDebug($"Vote SAN: {voteSAN}");

            Participant? participant = null;
            Vote? vote = null;
            Game? game = null;

            using (var db = dbo.GetDb())
            {
                try
                {
                    // participant and vote
                    log.LogDebug($"Find or create participant...");
                    participant = await dbo.FindOrCreateParticipantAsync(db, origin);
                    vote = new Vote()
                    {
                        MoveText = voteSAN,
                        Participant = participant,
                        NetworkMovePostId = origin.SourcePostId,
                    };
                    log.LogDebug($"Participant: {JsonConvert.SerializeObject(participant)}");
                    log.LogDebug($"Vote: {JsonConvert.SerializeObject(vote)}");

                    // check the game
                    log.LogDebug("Establishing game for the post.");
                    game = dbo.GetActiveGameForCurrentBoardResponse(db, origin); // throws GameNotFoundException

                    // establish wether this participant is permitted to vote on this move
                    var mayVote = gm.ParticipantOnSide(game, participant);
                    if (!mayVote)
                    {
                        throw new VoteRejectionException(vote, VoteValidationState.OffSide, origin);
                    }

                    // check validity of vote SAN
                    var newBoard = gm.ValidateSAN(game.CurrentBoard, vote); // throws VoteRejectionException
                    vote.ValidationState = VoteValidationState.Valid;

                    // supercede any pre-existing vote
                    var preexistingVote = gm.GetCurrentValidVote(game.CurrentMove, participant);
                    if (preexistingVote != null)
                    {
                        log.LogDebug("Marking preexisting vote superceded.");
                        preexistingVote.ValidationState = VoteValidationState.Superceded;
                        log.LogDebug(JsonConvert.SerializeObject(preexistingVote));
                    }

                    // update participant with current side for this game
                    var commitment = participant.Commitments
                        .SingleOrDefault(c => c.GameShortcode == game.Shortcode && c.GameSide == game.CurrentSide);
                    if (commitment == null)
                    {
                        log.LogDebug("Recording new commitment for participant...");
                        commitment = new Commitment();
                        vote.Participant.Commitments.Add(commitment);
                    }
                    else
                    {
                        log.LogDebug("Updating commitment for participant...");
                    }
                    commitment.GameShortcode = game.Shortcode;
                    commitment.GameSide = game.CurrentSide;
                    log.LogDebug(JsonConvert.SerializeObject(commitment));

                    // record new vote on game
                    game.CurrentMove.Votes.Add(vote);
                    await db.SaveChangesAsync();

                    // post validation response, and attach to vote
                    var reply = new PostBuilder(PostType.MoveAccepted)
                        .InReplyTo(origin)
                        .Build();

                    var validationPost = await social.PostAsync(reply);
                    vote.ValidationPost = validationPost;
                    await db.SaveChangesAsync();
                }
                catch (GameNotFoundException e)
                {
                    var summary = $"No game linked to move post from {e.Command.SourceUsername.Full}: {voteSAN}";
                    log.LogWarning(summary);

                    // TODO: remove - this is messy logging to try and catch the issue
                    log.LogWarning(JsonConvert.SerializeObject(db.Games.ToList()));

                    var reply = new PostBuilder(PostType.GameNotFound)
                        .WithGameNotFoundReason(e.Reason)
                        .InReplyTo(origin)
                        .Build();

                    await social.PostAsync(reply);
                }
                catch (VoteRejectionException e)
                {
                    
                    var summary = $"{e.Reason} from {participant?.Username.Full ?? "unknown"}: {voteSAN}, {e.Detail}";
                    log.LogWarning(summary);

                    var reply = new PostBuilder(PostType.MoveValidation)
                        .WithValidationState(e.Reason)
                        .WithUsername(participant?.Username)
                        .WithSAN(voteSAN)
                        .WithDetail(e?.Detail)
                        .InReplyTo(origin)
                        .Build();

                    var post = await social.PostAsync(reply);

                    vote!.ValidationState = e.Reason;
                    vote!.ValidationPost = post;

                    db.Games.Attach(game!);
                    game!.CurrentMove.Votes.Add(vote);
                    await db.SaveChangesAsync();
                }
            } // db
        }

        private async Task ShutdownAsync(SocialCommand origin, IEnumerable<string> words)
        {
            log.LogInformation($"Shutting down.");
            polling = false;
            pollingCancellation?.Cancel();
        }

        protected override async Task FinishAsync()
        {
            log.LogDebug("FinishAsync");
        }

    }
}

