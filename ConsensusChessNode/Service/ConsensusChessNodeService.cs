using System;
using System.Collections;
using ConsensusChessShared.Service;

namespace ConsensusChessNode.Service
{
    public class ConsensusChessNodeService : AbstractConsensusService
    {
        protected override TimeSpan PollPeriod => TimeSpan.FromMinutes(1);

        public ConsensusChessNodeService(ILogger log, IDictionary env) : base(log, env)
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

        protected override async Task FinishAsync(CancellationToken cancellationToken)
        {
            log.LogWarning("FinishAsync not implemented");
        }

    }
}

