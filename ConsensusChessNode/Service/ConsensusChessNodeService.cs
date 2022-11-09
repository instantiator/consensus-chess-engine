using System;
using System.Collections;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;

namespace ConsensusChessNode.Service
{
    public class ConsensusChessNodeService : AbstractConsensusService
    {
        protected override TimeSpan PollPeriod => TimeSpan.FromMinutes(1);
        protected override NodeType NodeType => NodeType.Node;

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

        private async Task ShutdownAsync(SocialCommand origin, IEnumerable<string> words)
        {
            log.LogInformation($"Shutting down.");
            polling = false;
        }

        protected override async Task FinishAsync()
        {
            log.LogWarning("FinishAsync not implemented");
        }

    }
}

