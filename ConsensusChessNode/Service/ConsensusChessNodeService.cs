using System;
using System.Collections;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Helpers;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Mastonet.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static ConsensusChessShared.Content.BoardFormatter;

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
            await CheckAndPostUnpostedBoardsAsync();
            await CheckAndPostUnpostedEndedGamesAsync();
        }

        private async Task CheckAndPostUnpostedBoardsAsync()
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
                            .WithBoard(board, BoardFormat.StandardFAN)
                            .Build();
                        var posted = await social.PostAsync(post);
                        board.BoardPosts.Add(posted);

                        log.LogDebug("Saving board and new board posts...");
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task CheckAndPostUnpostedEndedGamesAsync()
        {
            using (var db = dbo.GetDb())
            {
                // find and post unposted ended/abandoned games
                var unpostedEndedGames = gm.FindUnpostedEndedGames(db.Games, state.Shortcode);
                foreach (var game in unpostedEndedGames)
                {
                    log.LogDebug($"Game {game.Shortcode} in state {game.State} - posting about it...");
                    Post post;
                    switch (game.State)
                    {
                        case GameState.Abandoned:
                            post = new PostBuilder(PostType.Node_GameAbandonedUpdate)
                                .WithGame(game)
                                .Build();
                            break;
                        case GameState.Stalemate:
                        case GameState.BlackKingCheckmated:
                        case GameState.WhiteKingCheckmated:
                            post = new PostBuilder(PostType.Node_GameEndedUpdate)
                                .WithGame(game)
                                .Build();
                            break;
                        default:
                            log.LogWarning($"Should not have attempted to post the end of game in state: {game.State}");
                            continue;
                    }
                    var posted = await social.PostAsync(post);
                    game.GamePosts.Add(posted);

                    log.LogDebug("Saving new game post...");
                    await db.SaveChangesAsync();
                }
            }
        }

        protected override void RegisterForCommands(CommandProcessor processor)
        {
            processor.Register("shutdown", requireAuthorised: true, runsRetrospectively: false, ShutdownAsync);
            processor.Register("move", requireAuthorised: false, runsRetrospectively: true, ProcessVoteAsync);
            processor.RegisterUnrecognised(requireAuthorised: false, runsRetrospectively: true, AcceptUnrecognisedCommandAsync);
        }

        private async Task<bool> AcceptUnrecognisedCommandAsync(SocialCommand origin)
        {
            var skips = social.CalculateCommandSkips();
            var words = CommandHelper.ParseSocialCommand(origin.RawText, skips);
            var reconstructed = string.Join(" ", words);

            using (var db = dbo.GetDb())
            {
                var game = dbo.GetActiveGameForCurrentBoardResponse(db, origin);
                if (game != null)
                {
                    var ok = MoveFormatter.LooksLikeMoveText(game.CurrentBoard, reconstructed);
                    if (ok) await ProcessVoteAsync(origin, reconstructed);
                    return ok;
                }
                return false;
            }
        }

        private async Task ProcessVoteAsync(SocialCommand origin, IEnumerable<string> words)
        {
            log.LogDebug($"Processing vote from {origin.SourceUsername.Full}, in reply to {origin.InReplyToId?.ToString() ?? "(none)"}: {string.Join(" ", words)}");

            var moveText = string.Join(" ", words.Skip(1));
            log.LogDebug($"Vote move text: {moveText}");
            await ProcessVoteAsync(origin, moveText);
        }

        private async Task ProcessVoteAsync(SocialCommand origin, string moveText)
        {
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
                    vote = new Vote(
                        postId: origin.SourcePostId,
                        raw: moveText,
                        participant: participant);

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
                    vote.MoveSAN = gm.NormaliseAndValidateMoveTextToSAN(game.CurrentBoard, vote); // throws VoteRejectionException
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
                    var summary = $"No game linked to move post from {e.Command.SourceUsername.Full}: {moveText}";
                    log.LogWarning(summary);

                    var reply = new PostBuilder(PostType.GameNotFound)
                        .WithGameNotFoundReason(e.Reason)
                        .InReplyTo(origin)
                        .Build();

                    await social.PostAsync(reply);
                }
                catch (VoteRejectionException e)
                {
                    var summary = $"{e.Reason} from {participant?.Username.Full ?? "unknown"}: {moveText}, {e.Detail}";
                    log.LogWarning(summary);

                    var reply = new PostBuilder(PostType.MoveValidation)
                        .WithValidationState(e.Reason)
                        .WithUsername(participant?.Username)
                        .WithMoveText(moveText)
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

