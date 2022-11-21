using System;
using System.Collections;
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
            // TODO: check each game, if the current move has expired tally votes and recalculate the board
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
                    words,
                    origin.NetworkUserId,
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

                    // TODO: don't forget to update (or add!) an integration test

                    // TODO: update summary
                    var summary = $"New {game.SideRules} game for: {string.Join(", ", nodeShortcodes)}";
                    log.LogInformation(summary);

                    var post = new PostBuilder(PostType.GameAnnouncement)
                        .WithGame(game)
                        .Build();

                    await social.PostAsync(post);

                    var reply = new PostBuilder(PostType.CommandResponse)
                        .WithText(summary)
                        .InReplyTo(origin.SourceId)
                        .Build();

                    await social.PostAsync(reply);
                }
                else
                {
                    var unrecognised = nodeShortcodes.Where(network => !db.NodeState.Any(ns => ns.Shortcode == network));
                    log.LogWarning($"New game node shortcodes unrecognised: {string.Join(", ",unrecognised)}");

                    var summary = $"Unrecognised shortcodes: {string.Join(", ", unrecognised)}";
                    var reply = new PostBuilder(PostType.CommandResponse)
                        .WithText(summary)
                        .InReplyTo(origin.SourceId)
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

