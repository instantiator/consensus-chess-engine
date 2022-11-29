using System;
using System.Collections;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Helpers;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ConsensusChessEngine.Service
{
    public class ConsensusChessEngineService : AbstractConsensusService
    {
        // TODO: be a good citizen - set a polling period that isn't too disruptive
        private static readonly TimeSpan DefaultPollPeriod = TimeSpan.FromMinutes(1);
        private TimeSpan? overridePollPeriod;
        protected override TimeSpan PollPeriod => overridePollPeriod ?? DefaultPollPeriod;
        protected override NodeType NodeType => NodeType.Engine;

        public ConsensusChessEngineService(ILogger log, ServiceIdentity id, DbOperator dbo, Network network, ISocialConnection social, TimeSpan? overridePollPeriod = null)
            : base(log, id, dbo, network, social)
        {
            this.overridePollPeriod = overridePollPeriod;
        }

        protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            IEnumerable<Game> gamesToMove;
            using (var db = dbo.GetDb())
            {
                gamesToMove = dbo.GetActiveGamesWithExpiredMoves(db, null);
                foreach (var game in gamesToMove)
                {
                    await AdvanceGameAsync(game);
                    await db.SaveChangesAsync();
                }
            }
        }

        private async Task AdvanceGameAsync(Game game)
        {
            // count all votes
            var votes = gm.CountVotes(game.CurrentMove);
            var summary = string.Join(
                "\n",
                votes
                    .OrderByDescending(pair => pair.Value)
                    .Select(pair => $"{pair.Key}: {pair.Value}"));
            log.LogDebug($"Votes for {game.Shortcode}:\n{summary}");
            // TODO(IGC-69): intervention point to post stats about the game

            // determine next move for the game, and apply it
            var nextSAN = gm.NextMoveFor(votes);
            if (nextSAN != null)
            {
                log.LogWarning($"Advancing game: {game.Shortcode} with move: {nextSAN}");
                gm.AdvanceGame(game, nextSAN);

                Post? post;
                switch (game.State)
                {
                    case GameState.InProgress:
                        post = PostBuilder.Engine_GameAdvance(game).Build();
                        break;
                    case GameState.WhiteKingCheckmated:
                    case GameState.BlackKingCheckmated:
                    case GameState.Stalemate:
                        post = PostBuilder.Engine_GameEnded(game).Build();
                        break;
                    default:
                        post = null;
                        break;
                }
                if (post != null)
                {
                    if (!game.GamePosts.Any(p
                        => p.Type == post.Type
                        && post.NodeShortcode == identity.Shortcode
                        && p.Succeeded))
                    {
                        var posted = await social.PostAsync(post);
                        game.GamePosts.Add(posted);
                    }
                }
            }
            else
            {
                log.LogWarning($"0 votes found for game: {game.Shortcode}");
                log.LogInformation($"Deactivating game: {game.Shortcode}");
                gm.AbandonGame(game);
                var post = PostBuilder.Engine_GameAbandoned(game).Build();
                var posted = await social.PostAsync(post);
                game.GamePosts.Add(posted);
            }
        }

        protected override void RegisterForCommands(CommandProcessor processor)
        {
            processor.Register("shutdown", requireAuthorised: true, runsRetrospectively: false, ShutdownAsync);
            processor.Register("new", requireAuthorised: true, runsRetrospectively: true, StartNewGameAsync);
        }

        private async Task StartNewGameAsync(SocialCommand origin, IEnumerable<string> words)
        {
            // TODO: more complex games, better structure for issuing commands
            var nodeShortcodes = words.Skip(1); // everything after "new" is a shortcode (for now)

            if (nodeShortcodes.Count() == 0)
            {
                var summary = "No sides provided - cannot create game.";
                log.LogWarning(summary);
                throw new CommandRejectionException(
                    origin,
                    words,
                    CommandRejectionReason.CommandMalformed,
                    summary);
            }

            using (var db = dbo.GetDb())
            {
                var nodesOk = nodeShortcodes.All(nodeShortcode => db.NodeState.Any(ns => ns.Shortcode == nodeShortcode));

                // TODO: participantNetworks aren't actually required for move-lock games - remove and test without
                var participantNetworks = db.NodeState.Where(ns => nodeShortcodes.Contains(ns.Shortcode)).Select(ns => ns.Network);

                if (nodesOk)
                {
                    var shortcode = dbo.GenerateUniqueGameShortcode(db);
                    var game = gm.CreateSimpleMoveLockGame(shortcode, "simple move-lock game",
                        participantNetworkServers: participantNetworks.Select(n => n.NetworkServer),
                        postingNodeShortcodes: nodeShortcodes);
                    db.Games.Add(game);
                    await db.SaveChangesAsync();

                    var summary = $"New {game.SideRules} game for: {string.Join(", ", nodeShortcodes)}";
                    log.LogInformation(summary);

                    var post = PostBuilder.Engine_GameAnnouncement(game).Build();
                    await social.PostAsync(post);
                    game.GamePosts.Add(post);
                    db.Update(game);
                    await db.SaveChangesAsync();

                    var reply = PostBuilder.Engine_GameCreationResponse(game).InReplyTo(origin).Build();
                    await social.PostAsync(reply);
                }
                else
                {
                    var unrecognised = nodeShortcodes.Where(network => !db.NodeState.Any(ns => ns.Shortcode == network));
                    log.LogWarning($"New game node shortcodes unrecognised: {string.Join(", ",unrecognised)}");

                    var summary = $"Unrecognised shortcodes: {string.Join(", ", unrecognised)}";
                    var reply = PostBuilder.CommandRejection(CommandRejectionReason.CommandMalformed, unrecognised)
                        .InReplyTo(origin)
                        .Build();
                    await social.PostAsync(reply);
                }

                ReportOnGames();
            }
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

