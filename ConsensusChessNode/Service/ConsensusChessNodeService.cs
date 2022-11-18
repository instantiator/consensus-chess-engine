using System;
using System.Collections;
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
                var unpostedBoardChecks = gm.FindUnpostedBoards(db.Games, state.Shortcode);
                foreach (var check in unpostedBoardChecks)
                {
                    var game = check.Key;
                    var board = check.Value;

                    if (board != null)
                    {
                        log.LogInformation($"Found a new board to post in game: {game.Id}");
                        var posted = await social.PostAsync(game, board);
                        board.BoardPosts.Add(posted);

                        log.LogDebug("Saving board and new board posts...");
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        protected override void RegisterForCommands(CommandProcessor processor)
        {
            processor.Register("shutdown", requireAuthorised: true, runsRetrospectively: false, ShutdownAsync);
            processor.Register("move", requireAuthorised: false, runsRetrospectively: true, ProcessVoteAsync);
        }

        private async Task ProcessVoteAsync(SocialCommand origin, IEnumerable<string> words)
        {
            log.LogDebug($"Processing vote from {origin.SourceAccount}, in reply to {origin.InReplyToId?.ToString() ?? "(none)"}: {string.Join(" ", words)}");

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
                        NetworkMovePostId = origin.SourceId!.Value,
                    };
                    log.LogDebug($"Participant: {JsonConvert.SerializeObject(participant)}");
                    log.LogDebug($"Vote: {JsonConvert.SerializeObject(vote)}");

                    // check the game
                    log.LogDebug("Establishing game for the post.");
                    game = dbo.GetGameForVote(db, origin); // throws GameNotFoundException

                    // establish wether this participant is permitted to vote on this move
                    var mayVote = gm.ParticipantMayVote(game, participant);
                    if (!mayVote)
                    {
                        throw new VoteRejectionException(vote, VoteValidationState.NotPermitted, origin);
                    }

                    // check validity of vote SAN
                    var newBoard = gm.ValidateSAN(game.CurrentBoard, vote); // throws VoteRejectionException
                    vote.ValidationState = VoteValidationState.Valid;

                    // supercede any pre-existing vote
                    var preexistingVote = gm.GetCurrentValidVote(game, participant);
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
                    var validationPost = await social.ReplyAsync(origin, "Move accepted - thank you", PostType.MoveValidation);
                    vote.ValidationPost = validationPost;
                    await db.SaveChangesAsync();
                }
                catch (GameNotFoundException e)
                {
                    var summary = $"No game linked to move post from {e.Command.SourceAccount}: {voteSAN}";
                    log.LogWarning(summary);

                    // TODO: remove - this is messy logging to try and catch the issue
                    log.LogWarning(JsonConvert.SerializeObject(db.Games.ToList()));
                    await social.ReplyAsync(origin, summary, PostType.MoveValidation);
                }
                catch (VoteRejectionException e)
                {
                    var summary = $"{e.Reason} from {e.Command?.SourceAccount ?? "unknown"}: {voteSAN}, {e.Message}";
                    log.LogWarning(summary);

                    var post = await social.ReplyAsync(origin, summary, PostType.MoveValidation);
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
            pollingCancellation.Cancel();
        }

        protected override async Task FinishAsync()
        {
            log.LogDebug("FinishAsync");
        }

    }
}

