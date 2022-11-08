using System;
using System.Collections;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;

namespace ConsensusChessEngine.Service
{
    public class ConsensusChessEngineService : AbstractConsensusService
    {
        public ConsensusChessEngineService(ILogger log, IDictionary env) : base(log, env)
        {
        }

        private bool waiting;

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            waiting = true;
            await social.StartListeningForCommandsAsync(CommandReceived, null);
            while (!cancellationToken.IsCancellationRequested && waiting)
            {
                log.LogDebug("Waiting for commands...");
                await Task.Delay(1000, cancellationToken); // snooze
            }
        }

        private async Task CommandReceived(SocialCommand command)
        {
            log.LogInformation($"Command received: {command.RawText}");

            if (command.RawText.ToLower().Contains("shutdown"))
            {
                log.LogInformation("shutdown command");
                waiting = false;
            }

            // TODO: interpret commands
            // waiting = false;
        }

        protected override async Task FinishAsync(CancellationToken cancellationToken)
        {
            await social.StopListeningForCommandsAsync(CommandReceived);
        }
    }
}

