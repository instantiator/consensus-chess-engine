using System;
using System.Collections;
using ConsensusChessShared.Service;

namespace ConsensusChessEngine.Service
{
    public class ConsensusChessEngineService : AbstractConsensusService
    {
        public ConsensusChessEngineService(ILogger log, IDictionary env) : base(log, env)
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

