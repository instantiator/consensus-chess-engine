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
        protected override TimeSpan PollPeriod => TimeSpan.FromSeconds(15);
        protected override NodeType NodeType => NodeType.Node;

        private DbOperator dbOperator;

        public ConsensusChessNodeService(ILogger log, IDictionary env) : base(log, env)
        {
            dbOperator = new DbOperator(env);
        }

        protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            using (var db = GetDb())
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

            Game? game = null;
            Vote? vote = null;
            Participant? participant = null;

            var voteSAN = string.Join(" ", words.Skip(1));
            log.LogDebug($"Vote SAN: {voteSAN}");

            participant = await dbOperator.FindOrCreateParticipantAsync(origin);
            log.LogDebug($"Participant: {JsonConvert.SerializeObject(participant)}");

            vote = new Vote()
            {
                MoveText = voteSAN,
                Participant = participant,
                NetworkMovePostId = origin.SourceId!.Value,
            };

            try
            {
                log.LogDebug("Establishing game for the post.");
                game = dbOperator.GetGameForVote(origin); // throws GameNotFoundException
                var newBoard = gm.ValidateSAN(game.CurrentBoard, voteSAN); // throws VoteRejectionException
                var mayVote = gm.ParticipantMayVote(game, participant);
                if (!mayVote) { throw new VoteRejectionException(VoteValidationState.NotPermitted, origin, "Not permitted to vote on this move."); }

                // no exceptions thrown - this is a successful vote
                using (var db = GetDb())
                {
                    // set vote valid
                    vote.ValidationState = VoteValidationState.Valid;

                    // update participant with current side for this game
                    var commitment = participant.Commitments
                        .SingleOrDefault(c => c.GameShortcode == game.Shortcode && c.GameSide == game.CurrentSide);

                    if (commitment == null)
                    {
                        commitment = new Commitment()
                        {
                            GameShortcode = game.Shortcode,
                            GameSide = game.CurrentSide
                        };
                        vote.Participant.Commitments.Add(commitment);
                    }

                    // attach vote to game
                    db.Attach(game); // will this work?
                    game.CurrentMove.Votes.Add(vote);
                    await db.SaveChangesAsync();
                }

                // post validation response, and attach to vote
                var validationPost = await social.ReplyAsync(origin, "Move accepted - thank you", PostType.MoveValidation);

                using (var db = GetDb())
                {
                    db.Attach(game); // will this work?
                    db.Attach(vote);
                    vote.ValidationPost = validationPost;
                    db.Update(vote);
                    await db.SaveChangesAsync();
                }
            }
            catch (GameNotFoundException e)
            {
                vote.ValidationState = VoteValidationState.NoGame;
                var summary = $"No game linked to move post from {e.Command.SourceAccount}: {voteSAN}";
                log.LogWarning(summary);

                // TODO: remove - this is messy logging to try and catch the issue
                using (var db = GetDb())
                    log.LogWarning(JsonConvert.SerializeObject(db.Games.ToList()));

                await social.ReplyAsync(origin, summary, PostType.MoveValidation);
            }
            catch (VoteRejectionException e)
            {
                var summary = $"{e.Reason} from {e.Command?.SourceAccount ?? "unknown"}: {voteSAN}, {e.Message}";
                log.LogWarning(summary);

                var post = await social.ReplyAsync(origin, summary, PostType.MoveValidation);
                vote.ValidationState = e.Reason;
                vote.ValidationPost = post;

                using (var db = GetDb())
                {
                    db.Games.Attach(game!);
                    game!.CurrentMove.Votes.Add(vote);
                    await db.SaveChangesAsync();
                }
            }
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

