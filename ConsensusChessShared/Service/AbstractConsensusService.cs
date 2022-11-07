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
        protected IDictionary env;

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
            log.LogDebug("StartAsync at: {time}", DateTimeOffset.Now);
            running = true;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            log.LogDebug("ExecuteAsync at: {time}", DateTimeOffset.Now);

            try
            {
                log.LogDebug($"Display name: {await social.GetDisplayNameAsync()}");
                log.LogDebug($"Active games: {db.Games.ToList().Count(g => g.Active)}");
                var posted = await social.PostAsync(SocialStatus.Started);

                await RunAsync(cancellationToken);
            }
            finally
            {
                running = false;
                await FinishAsync(cancellationToken);
                log.LogInformation("ExecuteAsnyc complete at: {time}", DateTimeOffset.Now);
            }
        }

        protected abstract Task RunAsync(CancellationToken cancellationToken);

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            log.LogDebug("StopAsync");
            await social.PostAsync(SocialStatus.Stopped);

            if (running)
            {
                running = false;
                await FinishAsync(cancellationToken);
            }
        }

        protected abstract Task FinishAsync(CancellationToken cancellationToken);
    }
}

