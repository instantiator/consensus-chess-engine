using System;
using System.Collections;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Helpers;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ConsensusChessEngine.Service
{
    public class ConsensusChessEngineService : AbstractConsensusService
    {
        protected override TimeSpan PollPeriod => TimeSpan.FromMinutes(1);
        protected override NodeType NodeType => NodeType.Engine;

        public ConsensusChessEngineService(ILogger log, IDictionary env) : base(log, env)
        {
        }

        protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            // TODO: check each game, if the current move has expired tally votes and recalculate the board!
        }

        protected override void RegisterForCommands(CommandProcessor processor)
        {
            processor.Register("shutdown", true, ShutdownAsync);
            processor.Register("new", true, StartNewGameAsync);
        }

        private async Task StartNewGameAsync(SocialCommand origin, IEnumerable<string> words)
        {
            // TODO: more complex games, better structure for issuing commands
            var shortcodes = words.Skip(1); // everything after "new" is a shortcode (for now)

            if (shortcodes.Count() == 0)
            {
                log.LogWarning("No sides provided - cannot create game.");
            }

            using (var db = GetDb())
            {
                var networksOk = shortcodes.All(network => db.NodeStates.Any(ns => ns.Shortcode == network));

                if (networksOk)
                {
                    // TODO: not all game types will use the same nodes for both sides
                    var game = new Game(shortcodes, shortcodes, SideRules.MoveLock);

                    db.Games.Add(game);
                    await db.SaveChangesAsync();

                    var summary = $"New {game.SideRules} game for: {string.Join(", ", shortcodes)}";
                    log.LogInformation(summary);
                    await social.PostAsync(game);
                    await social.ReplyAsync(origin, summary);
                }
                else
                {
                    var unrecognised = shortcodes.Where(network => !db.NodeStates.Any(ns => ns.Shortcode == network));
                    log.LogWarning($"New game node shortcodes unrecognised: {string.Join(", ",unrecognised)}");
                    await social.ReplyAsync(origin, $"Unrecognised shortcodes: {string.Join(", ",unrecognised)}");
                }
                ReportOnGames();
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
        }

    }
}

