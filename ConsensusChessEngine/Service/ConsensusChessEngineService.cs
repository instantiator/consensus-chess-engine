using System;
using System.Collections;
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
        }

        protected override void RegisterForCommands(CommandProcessor processor)
        {
            processor.Register("shutdown", true, ShutdownAsync);
        }

        private async Task ShutdownAsync(IEnumerable<string> words)
        {
            log.LogInformation($"Shutting down.");
            polling = false;
        }

        protected override async Task FinishAsync()
        {
        }

    }
}

