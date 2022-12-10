using System;
using System.Collections;
using System.Diagnostics;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
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
        protected ServiceIdentity identity;
        protected ServiceConfig config;
        protected PostBuilderFactory posts;
        protected DbOperator dbo;
        protected EnumTranslator translator;

        protected CommandProcessor? cmd;
        protected GameManager gm;

        public bool Polling { get; protected set; }
        protected abstract TimeSpan PollPeriod { get; }
        protected CancellationTokenSource? pollingCancellation;

        public ISocialConnection Social => social;
        public bool Streaming => social?.Streaming ?? false;

        protected abstract NodeType NodeType { get; }

        protected DateTime startup;

        protected AbstractConsensusService(ILogger log, ServiceIdentity identity, DbOperator dbo, Network network, ISocialConnection social, ServiceConfig config)
        {
            this.log = log;
            this.identity = identity;
            this.dbo = dbo;
            this.network = network;
            this.social = social;
            this.config = config;
            this.posts = new PostBuilderFactory(config);
            this.startup = DateTime.Now;
            this.translator = new EnumTranslator();

            using (var db = dbo.GetDb())
            {
                dbo.InitDb(db);
            }

            gm = new GameManager(log);
            state = RegisterNode(network);
        }

        protected NodeState RegisterNode(Network net)
        {
            using (var db = dbo.GetDb())
            {

                log.LogDebug($"Registering node with db...");
                var currentState = db.NodeState.Where(s => s.Shortcode == identity.Shortcode).SingleOrDefault();
                if (currentState == null)
                {
                    currentState = new NodeState(identity.Name, identity.Shortcode, net);
                    db.NodeState.Add(currentState);
                    db.SaveChanges();
                }
                return currentState;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            log.LogInformation("StartAsync at: {time}", DateTimeOffset.Now);
            EraseHealthIndicators();

            await social.InitAsync(state);
            log.LogInformation($"Initialised as: {social.Username!.Full}");
            log.LogInformation($"Authorised accounts: {string.Join(", ", network.AuthorisedAccountsList)}");
            LogGameCount();

            var skips = social.CalculateCommandSkips();
            var ignorables = config.Ignorables;
            log.LogDebug($"Command prefix skips: {string.Join(", ", skips)}");
            log.LogDebug($"Ignorable keywords:   {string.Join(", ", ignorables)}");
            cmd = new CommandProcessor(log, network.AuthorisedAccountsList, social.Username, skips, ignorables);
            cmd.OnFailAsync += Cmd_OnFailAsync;
            RegisterForCommands(cmd);
            social.OnStateChange += RecordStateChangeAsync;

            // permit dependencies to start
            IndicateHealthReady();
        }

        private async Task Cmd_OnFailAsync(SocialCommand origin, string message, CommandRejectionReason reason)
        {
            var post = posts.CommandRejection(reason).InReplyTo(origin).Build();
            await social.PostAsync(post);
        }

        private async Task RecordStateChangeAsync(NodeState newState)
        {
            if (state.Id != newState.Id) { throw new InvalidOperationException($"state.Id = {state.Id}, newState.Id = {newState.Id}"); }

            using (var db = dbo.GetDb())
            {
                db.NodeState.Attach(state);

                // copy things that might change over?
                state.LastNotificationId = newState.LastNotificationId;
                state.StatePosts = newState.StatePosts;

                db.NodeState.Update(state);
                await db.SaveChangesAsync();
            }
        }

        private async Task RecordStatePostAsync(Post report)
        {
            using (var db = dbo.GetDb())
            {
                state.StatePosts.Add(report);
                db.NodeState.Attach(state);
                await db.SaveChangesAsync();
            }
        }

        protected void LogGameCount()
        {
            using (var db = dbo.GetDb())
            {
                log.LogDebug($"Active games: {db.Games.ToList().Count(g => g.Active)}");
            }
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            log.LogDebug("ExecuteAsync at: {time}", DateTimeOffset.Now);

            try
            {
                running = true;

                // post readiness
                var post = posts.SocialStatus(state, SocialStatus.Started).Build();
                var posted = await social.PostAsync(post);
                await RecordStatePostAsync(posted);

                // listen for commands
                await social.StartListeningForCommandsAsync(cmd!.ParseAsync, true);

                // poll for events
                pollingCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                Polling = true;
                while (!pollingCancellation.Token.IsCancellationRequested && Polling && running)
                {
                    log.LogTrace("Polling...");
                    await PollAsync(pollingCancellation.Token);
                    await Task.Delay(PollPeriod, pollingCancellation.Token); // snooze
                }

                log.LogDebug($"Run complete.");
            }
            finally
            {
                Polling = false;
                running = false;
                pollingCancellation?.Cancel();
                await FinishImplementationAsync();
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


        protected async Task PollAsync(CancellationToken cancellationToken)
        {
            // collect and parse commands
            // TODO: you are here
            await CheckAndParseCommands();
            await PollImplementationAsync(cancellationToken);
        }

        protected async Task CheckAndParseCommands()
        {
            log.LogDebug($"Checking for new notifications...");
            social.PauseStream();
            var commands = await social.GetAllNotificationSinceAsync(false, state.LastNotificationId, startup);
            social.ResumeStream();

            if (commands.Count() > 0)
            {
                log.LogDebug($"Found {commands.Count()} new commands for processing...");
                foreach (var command in commands)
                {
                    await social.ProcessCommandAsync(command);
                }
            }
        }

        protected abstract void RegisterForCommands(CommandProcessor processor);
        protected abstract Task PollImplementationAsync(CancellationToken cancellationToken);

        public async Task StopAsync()
        {
            EraseHealthIndicators();
            log.LogInformation("StopAsync at: {time}", DateTimeOffset.Now);

            await social.StopListeningForCommandsAsync(cmd!.ParseAsync);

            // post stopped
            var post = posts.SocialStatus(state, SocialStatus.Stopped).Build();
            var posted = await social.PostAsync(post);
            await RecordStatePostAsync(posted);

            // stop polling
            if (running)
            {
                running = false;
                await FinishImplementationAsync();
            }
        }

        protected abstract Task FinishImplementationAsync();
    }
}

