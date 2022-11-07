using System;
using System.Collections;
using ConsensusChessShared.Service;

namespace ConsensusChessNode.Service
{
    public class ConsensusChessNodeService : AbstractConsensusService
    {
        public ConsensusChessNodeService(ILogger log, IDictionary env) : base(log, env)
        {
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            log.LogWarning("RunAsync not implemented");
        }

        protected override async Task FinishAsync(CancellationToken cancellationToken)
        {
            log.LogWarning("FinishAsync not implemented");
        }
    }
}

