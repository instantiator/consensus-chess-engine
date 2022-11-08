using System;
using System.Collections;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Service
{
	public abstract class AbstractConsensusService
	{
        protected readonly HttpClient http = new HttpClient();
        protected ConsensusChessDbContext db;
        protected Network network;
        protected ISocialConnection social;
        protected ILogger log;
        protected bool running;
        protected bool polling;
        protected IDictionary env;
        protected CommandProcessor? cmd;

        protected abstract TimeSpan PollPeriod { get; }

        protected AbstractConsensusService(ILogger log, IDictionary env)
        {
            this.log = log;
            this.env = env;
            network = Network.FromEnvironment(NetworkType.Mastodon, env);
            social = SocialFactory.From(log, network);

            db = ConsensusChessDbContext.FromEnvironment(env);
            log.LogDebug($"Database context created.");

            var connection = db.Database.CanConnect();
            log.LogDebug($"Database connection: {connection}");

            db.Database.Migrate();
            log.LogDebug($"Migrations run.");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            log.LogInformation("StartAsync at: {time}", DateTimeOffset.Now);

            await social.InitAsync();
            log.LogDebug($"Display name: {social.DisplayName}");
            log.LogDebug($"Account name: {social.AccountName}");
            log.LogDebug($"Authorised accounts: {string.Join(", ", network.AuthorisedAccountsList)}");
            log.LogDebug($"Active games: {db.Games.ToList().Count(g => g.Active)}");

            cmd = new CommandProcessor(log, network.AuthorisedAccountsList, social.CalculateCommandSkips());
            RegisterForCommands(cmd);
        }

        CancellationTokenSource pollingCancellation;

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            log.LogDebug("ExecuteAsync at: {time}", DateTimeOffset.Now);

            try
            {
                running = true;

                // listen for commands
                await social.StartListeningForCommandsAsync(cmd.Parse, null);

                // post readiness
                var posted = await social.PostAsync(SocialStatus.Started);

                // poll for events
                pollingCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                polling = true;
                while (!pollingCancellation.Token.IsCancellationRequested && polling && running)
                {
                    log.LogTrace("Polling...");
                    await PollAsync(pollingCancellation.Token);
                    await Task.Delay(PollPeriod, pollingCancellation.Token); // snooze
                }

                log.LogDebug($"Run complete.");
            }
            finally
            {
                polling = false;
                running = false;
                pollingCancellation?.Cancel();
                await FinishAsync();
                log.LogInformation("ExecuteAsnyc complete at: {time}", DateTimeOffset.Now);
            }
        }

        protected abstract void RegisterForCommands(CommandProcessor processor);

        protected abstract Task PollAsync(CancellationToken cancellationToken);

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            log.LogInformation("StopAsync at: {time}", DateTimeOffset.Now);
            await social.StopListeningForCommandsAsync(cmd.Parse);
            await social.PostAsync(SocialStatus.Stopped);

            if (running)
            {
                running = false;
                await FinishAsync();
            }
        }

        protected abstract Task FinishAsync();
    }
}

