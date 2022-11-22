using System;
using System.Collections;
using System.Diagnostics;
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
        protected bool polling;
        protected ServiceIdentity identity;

        protected CommandProcessor? cmd;
        protected GameManager gm;

        protected abstract TimeSpan PollPeriod { get; }

        protected abstract NodeType NodeType { get; }

        protected DbOperator dbo;

        protected CancellationTokenSource? pollingCancellation;

        protected AbstractConsensusService(ILogger log, ServiceIdentity identity, DbOperator dbo, Network network, ISocialConnection social)
        {
            this.log = log;
            this.identity = identity;
            this.dbo = dbo;
            this.network = network;
            this.social = social;

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
            var post = new PostBuilder(PostType.CommandResponse)
                .WithText(message)
                .InReplyTo(origin)
                .Build();

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

        protected void ReportOnGames()
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
                var post = new PostBuilder(PostType.SocialStatus)
                    .WithNodeState(state)
                    .WithSocialStatus(SocialStatus.Started)
                    .Build();
                var posted = await social.PostAsync(post);
                await RecordStatePostAsync(posted);

                // listen for commands
                await social.StartListeningForCommandsAsync(cmd!.ParseAsync, true);

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

        public async Task StopAsync()
        {
            EraseHealthIndicators();
            log.LogInformation("StopAsync at: {time}", DateTimeOffset.Now);
            await social.StopListeningForCommandsAsync(cmd!.ParseAsync);
            var post = new PostBuilder(PostType.SocialStatus)
                .WithNodeState(state)
                .WithSocialStatus(SocialStatus.Stopped)
                .Build();
            var posted = await social.PostAsync(post);
            await RecordStatePostAsync(posted);

            if (running)
            {
                running = false;
                await FinishAsync();
            }
        }

        protected abstract Task FinishAsync();
    }
}

