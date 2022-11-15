using System;
using System.Collections;
using System.Diagnostics;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Service
{
	public abstract class AbstractConsensusService
	{
        public const string HEALTHCHECK_READY_PATH = "/tmp/health.ready";

        protected readonly HttpClient http = new HttpClient();
        protected ISocialConnection social;
        protected Network network;
        protected NodeState state;
        protected ILogger log;
        protected bool running;
        protected bool polling;
        protected IDictionary env;

        protected CommandProcessor? cmd;
        protected GameManager gm;

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


            gm = new GameManager(log);
            network = Network.FromEnvironment(env);

            state = RegisterNode(network);
            social = SocialFactory.From(log, network, state, network.DryRuns);
        }

        protected NodeState RegisterNode(Network net)
        {
            using (var db = GetDb())
            {
                var environment = env.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);
                var name = environment["NODE_NAME"];
                var shortcode = environment["NODE_SHORTCODE"];

                log.LogDebug($"Registering node with db...");
                var currentState = db.NodeState.Where(s => s.Shortcode == shortcode).SingleOrDefault();
                if (currentState == null)
                {
                    currentState = new NodeState(name, shortcode, net);
                    db.NodeState.Add(currentState);
                    db.SaveChanges();
                }
                return currentState;
            }
        }

        protected ConsensusChessDbContext GetDb() => ConsensusChessDbContext.FromEnvironment(env);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            log.LogInformation("StartAsync at: {time}", DateTimeOffset.Now);
            EraseHealthIndicators();

            await social.InitAsync();
            log.LogDebug($"Display name: {social.DisplayName}");
            log.LogDebug($"Account name: {social.AccountName}");
            log.LogDebug($"Authorised accounts: {string.Join(", ", network.AuthorisedAccountsList)}");
            ReportOnGames();

            var skips = social.CalculateCommandSkips();
            log.LogDebug($"Command prefix skips: {string.Join(", ", skips)}");
            cmd = new CommandProcessor(log, network.AuthorisedAccountsList, skips);
            cmd.OnFailAsync += Cmd_OnFailAsync;
            RegisterForCommands(cmd);
            social.OnStateChange += RecordStateChangeAsync;

            // permit dependencies to start
            IndicateHealthReady();
        }

        private async Task Cmd_OnFailAsync(SocialCommand origin, string message, CommandRejectionReason? reason)
        {
            await social.ReplyAsync(origin, message, PostType.CommandResponse);
        }

        private async Task RecordStateChangeAsync(NodeState newState)
        {
            if (state.Id != newState.Id) { throw new InvalidOperationException($"state.Id = {state.Id}, newState.Id = {newState.Id}"); }

            using (var db = GetDb())
            {
                // copy things that might change over?
                state.LastNotificationId = newState.LastNotificationId;
                state.StatePosts = newState.StatePosts;

                db.NodeState.Update(state);
                await db.SaveChangesAsync();
            }
        }

        private async Task RecordStatePostAsync(Post report)
        {
            using (var db = GetDb())
            {
                state.StatePosts.Add(report);
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

                // post readiness
                var posted = await social.PostAsync(SocialStatus.Started);
                await RecordStatePostAsync(posted);

                // listen for commands
                await social.StartListeningForCommandsAsync(cmd.ParseAsync, true);

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

        protected void IndicateHealthReady()
        {
            log.LogInformation("Healthcheck indicator: Ready");
            File.WriteAllText(
                HEALTHCHECK_READY_PATH,
                $"{DateTime.Now.ToUniversalTime().ToString("O")}");
        }

        protected void EraseHealthIndicators()
        {
            log.LogDebug("Deleting old healthcheck indicator.");
            if (File.Exists(HEALTHCHECK_READY_PATH))
                File.Delete(HEALTHCHECK_READY_PATH);
        }

        protected abstract void RegisterForCommands(CommandProcessor processor);

        protected abstract Task PollAsync(CancellationToken cancellationToken);

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            EraseHealthIndicators();
            log.LogInformation("StopAsync at: {time}", DateTimeOffset.Now);
            await social.StopListeningForCommandsAsync(cmd!.ParseAsync);
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

