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
        protected ISocialConnection social;
        protected NodeState state;
        protected Network network;
        protected ILogger log;
        protected bool running;
        protected bool polling;
        protected IDictionary env;
        protected CommandProcessor? cmd;

        protected abstract TimeSpan PollPeriod { get; }

        protected abstract NodeType NodeType { get; }

        protected AbstractConsensusService(ILogger log, IDictionary env)
        {
            this.log = log;
            this.env = env;

            using (var db = GetDb())
            {
                var connection = db.Database.CanConnect();
                log.LogDebug($"Database connection: {connection}");

                log.LogDebug($"Running migrations...");
                db.Database.Migrate();
            }

            state = RegisterNode();
            network = Network.FromEnvironment(env);
            social = SocialFactory.From(log, network, state);
        }

        protected NodeState RegisterNode()
        {
            using (var db = GetDb())
            {
                var environment = env.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);
                var name = environment["NODE_NAME"];

                log.LogDebug($"Registering node with db...");
                var currentState = db.NodeStates.Where(s => s.NodeName == name).SingleOrDefault();
                if (currentState == null)
                {
                    currentState = NodeState.Create(name);
                    db.NodeStates.Add(currentState);
                    db.SaveChanges();
                }
                return currentState;
            }
        }

        protected ConsensusChessDbContext GetDb() => ConsensusChessDbContext.FromEnvironment(env);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            log.LogInformation("StartAsync at: {time}", DateTimeOffset.Now);

            await social.InitAsync();
            log.LogDebug($"Display name: {social.DisplayName}");
            log.LogDebug($"Account name: {social.AccountName}");
            log.LogDebug($"Authorised accounts: {string.Join(", ", network.AuthorisedAccountsList)}");
            ReportOnGames();

            var skips = social.CalculateCommandSkips();
            log.LogDebug($"Command prefix skips: {string.Join(", ", skips)}");
            cmd = new CommandProcessor(log, network.AuthorisedAccountsList, skips);
            RegisterForCommands(cmd);
            social.OnStateChange += RecordStateChangeAsync;
        }

        private async Task RecordStateChangeAsync(NodeState newState)
        {
            if (state.Id != newState.Id) { throw new InvalidOperationException($"state.Id = {state.Id}, newState.Id = {newState.Id}"); }

            using (var db = GetDb())
            {
                // copy things that might change over?
                state.LastNotificationId = newState.LastNotificationId;
                state.StatePosts = newState.StatePosts;

                db.NodeStates.Update(state);
                await db.SaveChangesAsync();
            }
        }

        private async Task RecordStatePostAsync(PostReport report)
        {
            using (var db = GetDb())
            {
                state.StatePosts.Add(report);
                db.Add(report);
                db.NodeStates.Update(state);
                await db.SaveChangesAsync();
            }
        }

        protected void ReportOnGames()
        {
            using (var db = GetDb())
            {
                log.LogDebug($"Active games: {db.Games.ToList().Count(g => g.Active)}");
            }
        }

        protected CancellationTokenSource pollingCancellation;

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            log.LogDebug("ExecuteAsync at: {time}", DateTimeOffset.Now);

            try
            {
                running = true;

                // listen for commands
                await social.StartListeningForCommandsAsync(cmd.Parse, true);

                // post readiness
                var posted = await social.PostAsync(SocialStatus.Started);
                await RecordStatePostAsync(posted);

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
            var post = await social.PostAsync(SocialStatus.Stopped);
            await RecordStatePostAsync(post);

            if (running)
            {
                running = false;
                await FinishAsync();
            }
        }

        protected abstract Task FinishAsync();
    }
}

