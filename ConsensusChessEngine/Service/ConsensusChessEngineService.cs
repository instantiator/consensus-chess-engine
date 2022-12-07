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
using Newtonsoft.Json;
using static ConsensusChessShared.Content.BoardFormatter;
using static ConsensusChessShared.Content.BoardGraphicsData;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ConsensusChessEngine.Service
{
    public class ConsensusChessEngineService : AbstractConsensusService
    {
        // TODO: be a good citizen - set a polling period that isn't too disruptive
        private static readonly TimeSpan DefaultPollPeriod = TimeSpan.FromSeconds(30);
        private TimeSpan? overridePollPeriod;
        protected override TimeSpan PollPeriod => overridePollPeriod ?? DefaultPollPeriod;
        protected override NodeType NodeType => NodeType.Engine;

        public ConsensusChessEngineService(ILogger log, ServiceIdentity id, DbOperator dbo, Network network, ISocialConnection social, ServiceConfig config, TimeSpan? overridePollPeriod = null)
            : base(log, id, dbo, network, social, config)
        {
            this.overridePollPeriod = overridePollPeriod;
        }

        protected override async Task PollImplementationAsync(CancellationToken cancellationToken)
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

        protected override void RegisterForCommands(CommandProcessor processor)
        {
            processor.Register("shutdown", requireAuthorised: true, runsRetrospectively: false, ShutdownCmdAsync);
            processor.Register("new", requireAuthorised: true, runsRetrospectively: true, StartNewGameCmdAsync);
            processor.Register("status", requireAuthorised: true, runsRetrospectively: false, ShareStatusCmdAsync);
            processor.Register("abandon", requireAuthorised: true, runsRetrospectively: false, AbandonGameCmdAsync);
            processor.Register("advance", requireAuthorised: true, runsRetrospectively: false, AdvanceGameCmdAsync);
        }

        private async Task AdvanceGameCmdAsync(SocialCommand origin, IEnumerable<string> words)
        {
            if (words.Count() > 1)
            {
                var shortcode = words.ElementAt(1);
                using (var db = dbo.GetDb())
                {
                    var gamesList = db.Games.ToList();
                    var game = gamesList.SingleOrDefault(g => g.Shortcode == shortcode);

                    if (game != null)
                    {
                        await AdvanceGameAsync(game);
                        await db.SaveChangesAsync();
                        var reply = posts.CommandResponse($"Game advanced: {shortcode}")
                            .InReplyTo(origin)
                            .Build();
                        await social.PostAsync(reply);
                    }
                    else
                    {
                        var reply = posts.CommandRejection(CommandRejectionReason.CommandMalformed, new[] { shortcode })
                            .InReplyTo(origin)
                            .Build();
                        await social.PostAsync(reply);
                    }
                }
            }
            else
            {
                var reply = posts.CommandRejection(CommandRejectionReason.CommandMalformed)
                    .InReplyTo(origin)
                    .Build();
                await social.PostAsync(reply);
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
                        post = posts.Engine_GameAdvance(game).Build();
                        break;
                    case GameState.WhiteKingCheckmated:
                    case GameState.BlackKingCheckmated:
                    case GameState.Stalemate:
                        post = posts.Engine_GameEnded(game).Build();
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
                await DeactivateGameAsync(game);
            }
        }

        private async Task ShareStatusCmdAsync(SocialCommand origin, IEnumerable<string> words)
        {
            if (words.Count() > 1)
            {
                await ShareGameStatusAsync(origin, words);
            }
            else
            {
                await ShareGeneralStatusAsync(origin, words);
            }
        }

        private async Task ShareGameStatusAsync(SocialCommand origin, IEnumerable<string> words)
        {
            var shortcode = words.ElementAt(1);
            using (var db = dbo.GetDb())
            {
                var gamesList = db.Games.ToList();
                var game = gamesList.SingleOrDefault(g => g.Shortcode == shortcode);
                if (game != null)
                {
                    var header = $"{game.Shortcode}: \"{game.Title}\", {game.State}";
                    var moves = $"{game.Moves.Count()} Moves: {game.CurrentSide} active";
                    var votes = $"Votes: {string.Join(", ", game.Moves.Select(m => m.Votes.Count()))}";
                    //var serverList = new List<string>();
                    //serverList.AddRange(game.WhiteParticipantNetworkServers.Select(s => s.Value!));
                    //serverList.AddRange(game.BlackParticipantNetworkServers.Select(s => s.Value!));
                    //var servers = $"Servers: {string.Join(", ", serverList)}";
                    var shortcodeList = new List<string>();
                    shortcodeList.AddRange(game.BlackPostingNodeShortcodes.Select(s => s.Value!));
                    shortcodeList.AddRange(game.WhitePostingNodeShortcodes.Select(s => s.Value!));
                    var nodes = $"Nodes: {string.Join(", ",shortcodeList)}";

                    var status = string.Format($"{header}\n{nodes}\n{moves}\n{votes}");
                    log.LogInformation($"Game status information requested.\n{status}");

                    var post = posts.CommandResponse(status)
                        .WithGame(game)
                        .WithBoard(game.CurrentBoard, BoardFormat.StandardFAN)
                        .AndBoardGraphic(BoardStyle.PixelChess, BoardFormat.StandardFEN)
                        .InReplyTo(origin)
                        .Build();

                    await social.PostAsync(post);
                }
                else
                {
                    // no game found
                    var reply = posts.CommandRejection(CommandRejectionReason.CommandMalformed, new[] { shortcode })
                        .InReplyTo(origin)
                        .Build();
                    await social.PostAsync(reply);
                }
            }
        }

        private async Task ShareGeneralStatusAsync(SocialCommand origin, IEnumerable<string> words)
        {
            using (var db = dbo.GetDb())
            {
                var gamesList = db.Games.ToList();
                var nodesList = db.NodeState.ToList();

                var activeGames = gamesList
                    .Where(g => g.Active)
                    .OrderByDescending(g => g.Created)
                    .Select(g => $"- {g.Shortcode}: {g.State}");

                var inactiveGames = gamesList
                    .Where(g => !g.Active)
                    .OrderByDescending(g => g.Created)
                    .Select(g => $"- {g.Shortcode}: {g.State}");

                var nodes = nodesList.Select(ns => $"- {ns.Shortcode}: \"{ns.Name}\"");

                var status = string.Format(
                    "{0} nodes:\n{1}\n\n{2} active games:\n{3}\n\n{4} inactive games:\n{5}",
                    nodes.Count(),
                    string.Join("\n", nodes),
                    activeGames.Count(),
                    string.Join("\n", activeGames),
                    inactiveGames.Count(),
                    string.Join("\n", inactiveGames));

                log.LogInformation($"Status information requested.\n{status}");

                var post = posts.CommandResponse(status).InReplyTo(origin).Build();
                await social.PostAsync(post);
            }
        }

        private async Task StartNewGameCmdAsync(SocialCommand origin, IEnumerable<string> words)
        {
            // TODO: more complex games, better structure for issuing commands
            if (words.Count() < 5)
            {
                var summary = "new game command format: new MoveLock <title> <description> <node> [<node> [<node>]]...";
                log.LogWarning(summary);
                throw new CommandRejectionException(
                    origin,
                    words,
                    CommandRejectionReason.CommandMalformed,
                    summary);

            }

            // new <title> <description> <nodes...>
            var type = words.Skip(1).First();
            var title = words.Skip(2).First();
            var description = words.Skip(3).First();
            var nodeShortcodes = words.Skip(4);

            if (type != "MoveLock")
            {
                var summary = $"{type} unsupported. Only MoveLock games are currently supported.";
                log.LogWarning(summary);
                throw new CommandRejectionException(
                    origin,
                    words,
                    CommandRejectionReason.CommandMalformed,
                    summary);
            }

            if (nodeShortcodes.Count() == 0)
            {
                var summary = "No nodes provided - cannot create game.";
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
                    var game = gm.CreateSimpleMoveLockGame(
                        shortcode: shortcode,
                        title: title,
                        description: description,
                        participantNetworkServers: participantNetworks.Select(n => n.NetworkServer),
                        postingNodeShortcodes: nodeShortcodes);
                    db.Games.Add(game);
                    await db.SaveChangesAsync();

                    var summary = $"New {game.SideRules} game: {game.Title}\nNodes: {string.Join(", ", nodeShortcodes)}";
                    log.LogInformation(summary);

                    var post = posts.Engine_GameAnnouncement(game).Build();
                    await social.PostAsync(post);
                    game.GamePosts.Add(post);
                    db.Update(game);
                    await db.SaveChangesAsync();

                    var reply = posts.Engine_GameCreationResponse(game).InReplyTo(origin).Build();
                    await social.PostAsync(reply);
                }
                else
                {
                    var unrecognised = nodeShortcodes.Where(network => !db.NodeState.Any(ns => ns.Shortcode == network));
                    log.LogWarning($"New game node shortcodes unrecognised: {string.Join(", ",unrecognised)}");

                    var summary = $"Unrecognised shortcodes: {string.Join(", ", unrecognised)}";
                    var reply = posts.CommandRejection(CommandRejectionReason.CommandMalformed, unrecognised)
                        .InReplyTo(origin)
                        .Build();
                    await social.PostAsync(reply);
                }

                LogGameCount();
            }
        }

        private async Task AbandonGameCmdAsync(SocialCommand origin, IEnumerable<string> words)
        {
            var shortcode = words.ElementAt(1);

            using (var db = dbo.GetDb())
            {
                var game = db.Games.ToList().SingleOrDefault(g => g.Shortcode == shortcode);

                if (game != null)
                {
                    await DeactivateGameAsync(game);
                    await db.SaveChangesAsync();

                    var summary = $"Abandoned game: {game.Shortcode}";
                    var reply = posts.CommandResponse(summary).InReplyTo(origin).Build();
                    await social.PostAsync(reply);
                }    
                else
                {
                    // no game found
                    var reply = posts.CommandRejection(CommandRejectionReason.CommandMalformed, new[] { shortcode })
                        .InReplyTo(origin)
                        .Build();
                    await social.PostAsync(reply);
                }
            }
        }

        private async Task DeactivateGameAsync(Game game)
        {
            log.LogInformation($"Deactivating game: {game.Shortcode}");
            gm.AbandonGame(game);

            var post = posts.Engine_GameAbandoned(game).Build();
            var posted = await social.PostAsync(post);
            game.GamePosts.Add(posted);
        }

        private async Task ShutdownCmdAsync(SocialCommand origin, IEnumerable<string> words)
        {
            log.LogInformation($"Shutting down.");
            await StopAsync();
        }

        protected override async Task FinishImplementationAsync()
        {
            log.LogDebug("ConsensusChessEngineService.FinishImplementationAsync");
        }

    }
}

