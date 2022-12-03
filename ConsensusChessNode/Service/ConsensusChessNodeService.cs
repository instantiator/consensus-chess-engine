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
using static ConsensusChessShared.Content.BoardGraphicsData;

namespace ConsensusChessNode.Service
{
    public class ConsensusChessNodeService : AbstractConsensusService
    {
        // TODO: be a good citizen - set a polling period that isn't too disruptive
        private static readonly TimeSpan DefaultPollPeriod = TimeSpan.FromSeconds(30);
        private TimeSpan? overridePollPeriod;
        protected override TimeSpan PollPeriod => overridePollPeriod ?? DefaultPollPeriod;
        protected override NodeType NodeType => NodeType.Node;

        public ConsensusChessNodeService(ILogger log, ServiceIdentity id, DbOperator dbo, Network network, ISocialConnection social, ServiceConfig config, TimeSpan? overridePollPeriod = null)
            : base(log, id, dbo, network, social, config)
        {
            this.overridePollPeriod = overridePollPeriod;
        }

        protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            await CheckAndPostUnpostedBoardsAsync();
            await CheckAndPostUnpostedOverdueBoardRemindersAsync();
            await CheckAndPostUnpostedEndedGamesAsync();
        }

        private async Task CheckAndPostUnpostedOverdueBoardRemindersAsync()
        {
            using (var db = dbo.GetDb())
            {
                // find unposted board reminders for active games
                var unpostedBoards = gm.FindUnpostedActiveGameBoards(
                    db.Games,
                    state.Shortcode,
                    PostType.Node_BoardReminder);

                // establish which of these moves are due a reminder
                var overdueBoards = unpostedBoards
                    .Where(gb =>
                        DateTime.Now.ToUniversalTime() > gb.Key.CurrentMove.Deadline.Subtract(gb.Key.MoveReminder))
                    .ToDictionary(gb => gb.Key, gb => gb.Key.Moves.Single(m => m.From.Id == gb.Value.Id));

                // post the reminders
                foreach (var overdueGameBoard in overdueBoards)
                {
                    var game = overdueGameBoard.Key;
                    var move = overdueGameBoard.Value;
                    var board = move.From;

                    log.LogInformation($"Found a new board reminder to post in game: {game.Shortcode}");
                    var post = posts.Node_BoardReminder(game, board, move, BoardFormat.Words_en, BoardStyle.PixelChess).Build();
                    var posted = await social.PostAsync(post);
                    board.BoardPosts.Add(posted);

                    var instructional1 = posts.Node_VotingInstructions().InReplyTo(posted).Build();
                    var postedInstructional1 = await social.PostAsync(instructional1);
                    board.BoardPosts.Add(postedInstructional1);

                    var instructional2 = posts.Node_FollowInstructions().InReplyTo(postedInstructional1).Build();
                    var postedInstructional2 = await social.PostAsync(instructional2);
                    board.BoardPosts.Add(postedInstructional2);

                    log.LogDebug("Saving board and new board posts...");
                    await db.SaveChangesAsync();
                } // each overdue game/board reminder
            } // db
        }

        private async Task CheckAndPostUnpostedBoardsAsync()
        {
            using (var db = dbo.GetDb())
            {
                // find uposted board updates from active games
                var unpostedBoards = gm.FindUnpostedActiveGameBoards(
                    db.Games,
                    state.Shortcode,
                    PostType.Node_BoardUpdate);

                foreach (var gameBoard in unpostedBoards)
                {
                    var game = gameBoard.Key;
                    var board = gameBoard.Value;

                    log.LogInformation($"Found a new board update to post in game: {game.Shortcode}");
                    var post = posts.Node_BoardUpdate(game, board, BoardFormat.Words_en, BoardStyle.PixelChess).Build();
                    var posted = await social.PostAsync(post);
                    board.BoardPosts.Add(posted);

                    var instructional1 = posts.Node_VotingInstructions().InReplyTo(posted).Build();
                    var postedInstructional1 = await social.PostAsync(instructional1);
                    board.BoardPosts.Add(postedInstructional1);

                    var instructional2 = posts.Node_FollowInstructions().InReplyTo(postedInstructional1).Build();
                    var postedInstructional2 = await social.PostAsync(instructional2);
                    board.BoardPosts.Add(postedInstructional2);

                    log.LogDebug("Saving board and new board posts...");
                    await db.SaveChangesAsync();
                } // each game/board update to post
            } // db
        }

        private async Task CheckAndPostUnpostedEndedGamesAsync()
        {
            using (var db = dbo.GetDb())
            {
                // find and post unposted ended/abandoned games
                var unpostedEndedGames = gm.FindUnpostedEndedGames(db.Games, state.Shortcode, new[] { PostType.Node_GameAbandonedUpdate, PostType.Node_GameEndedUpdate });
                foreach (var game in unpostedEndedGames)
                {
                    log.LogDebug($"Game {game.Shortcode} in state {game.State} - posting about it...");
                    Post post;
                    switch (game.State)
                    {
                        case GameState.Abandoned:
                            post = posts.Node_GameAbandonedUpdate(game).Build();
                            break;
                        case GameState.Stalemate:
                        case GameState.BlackKingCheckmated:
                        case GameState.WhiteKingCheckmated:
                            post = posts.Node_GameEndedUpdate(game).Build();
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
                var game = dbo.GetActiveGameForCurrentBoardResponse(db, identity.Shortcode, origin);
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
                    game = dbo.GetActiveGameForCurrentBoardResponse(db, identity.Shortcode, origin); // throws GameNotFoundException

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

                        // prepare and post a reply to the original vote to let the user know it was superceded
                        // it's not saved to the vote, this is just a courtesy
                        var supercessionPost = posts.Node_MoveSuperceded(game.CurrentMove, game, game.CurrentSide, preexistingVote, vote)
                            .InReplyTo(preexistingVote.NetworkMovePostId, preexistingVote.Participant.Username)
                            .Build();

                        await social.PostAsync(supercessionPost);
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
                    var reply = posts.Node_MoveAccepted(game.CurrentMove, game, game.CurrentSide, vote)
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

                    var reply = posts.Node_GameNotFound(e.Reason)
                        .InReplyTo(origin)
                        .Build();
                    await social.PostAsync(reply);
                }
                catch (VoteRejectionException e)
                {
                    var summary = $"{e.Reason} from {participant?.Username.Full ?? "unknown"}: {moveText}, {e.Detail}";
                    log.LogWarning(summary);

                    // game is safe to use - if it were null, we'd see GameNotFoundException
                    var reply = posts.Node_MoveValidation(game!.CurrentMove, e.Reason, participant!.Username, moveText, e?.Detail)
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

