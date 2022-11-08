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

        private async Task StartNewGameAsync(IEnumerable<string> words)
        {
            var networks = words.Skip(1);

            if (networks.Count() == 0)
            {
                log.LogWarning("No sides provided - cannot create game.");
            }

            var game = new Game(networks, networks, SideRules.MoveLock);

            log.LogInformation($"New MoveLock game for: {string.Join(", ",networks)}");

            using (var db = GetDb())
            {
                db.Games.Add(game);
                await db.SaveChangesAsync();
            }

            ReportOnGames();
        }

        private async Task ShutdownAsync(IEnumerable<string> words)
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

