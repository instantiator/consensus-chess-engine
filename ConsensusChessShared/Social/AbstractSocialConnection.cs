using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using Mastonet.Entities;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public abstract class AbstractSocialConnection : ISocialConnection
	{
		public event Func<NodeState, Task> OnStateChange;

        public bool Ready { get; private set; }

		protected ILogger log;
		protected Network network;
		protected NodeState state;
		protected bool dryRuns;

        protected event Func<SocialCommand, Task>? asyncCommandReceivers;
        protected IEnumerable<SocialCommand>? missedCommands;

        public AbstractSocialConnection(ILogger log, Network network)
		{
			this.log = log;
			this.network = network;
			this.dryRuns = network.DryRuns;
            this.Ready = false;
		}

		public async Task InitAsync(NodeState state)
        {
            this.state = state;
            await InitImplementationAsync();
            Ready = true;
        }

        protected abstract Task InitImplementationAsync();

        public abstract SocialUsername? Username { get; set; }

		public abstract IEnumerable<string> CalculateCommandSkips();
        protected abstract Task GetMissedCommands();
        protected abstract Task MarkCommandProcessedAsync(long id);
        protected abstract Task StartListeningForNotificationsAsync();
        protected abstract Task<IEnumerable<Notification>> GetAllNotificationSinceAsync(long sinceId);
        public abstract Task StopListeningForCommandsAsync(Func<SocialCommand, Task> asyncReceiver);

        public async Task StartListeningForCommandsAsync(Func<SocialCommand, Task> asyncCommandReceiver, bool getMissedCommands)
		{
            asyncCommandReceivers += asyncCommandReceiver;
            if (getMissedCommands) { await GetMissedCommands(); }
            await StartListeningForNotificationsAsync();
            if (getMissedCommands) { await ProcessMissedCommands(); }
        }

        protected async Task ProcessMissedCommands()
        {
            if (missedCommands != null)
            {
                log.LogDebug($"Retrospectively processing {missedCommands.Count()} notifications...");
                foreach (var command in missedCommands)
                {
                    await ProcessCommand(command);
                }
            }
        }

        protected async Task ProcessCommand(SocialCommand command)
        {
            log.LogDebug(
                $"Processing {command.DeliveryMedium}:{command.DeliveryType} command." + "{0}",
                command.IsRetrospective ? " (retrospectively)" : "");

            log.LogDebug($"IsForMe: {command.IsForThisNode}, IsAuthorised: {command.IsAuthorised}, IsProcessed: {command.IsProcessed}");

            try
            {
                if (command.IsForThisNode && !command.IsProcessed)
                {
                    // now invoke the command (even if this fails we wouldn't want to re-run)
                    if (asyncCommandReceivers != null)
                    {
                        await asyncCommandReceivers.Invoke(command);
                    }
                    else
                    {
                        log.LogWarning("No receivers for this command.");
                    }
                }
                else
                {
                    log.LogWarning("Command received, but not for this node.");
                }
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unexpected exception processing command: {command.RawText}");
            }
            finally
            {

                // always mark the status as seen - we won't try again, even if execution fails
                if (command.IsForThisNode && !command.IsProcessed)
                {
                    await MarkCommandProcessedAsync(command.SourcePostId);
                    command.IsProcessed = true;
                }

                // always update the last notification id
                if (command.SourcePostId > state!.LastNotificationId)
                {
                    state.LastNotificationId = command.SourcePostId;
                    await ReportStateChangeAsync();
                }
            }
        }

        public async Task<Post> PostAsync(string text, PostType type = PostType.Unspecified, bool? dryRun = null)
		{
			log.LogInformation($"Posting: {text}");
            var post = new PostBuilder(type)
                .WithText(text)
                .Build();

            return await PostAsync(post, dryRun);
		}

        public abstract Task<Post> PostAsync(Post post, bool? dryRun);

        protected async Task ReportStateChangeAsync()
		{
			log.LogDebug($"ReportStateChange - new notification id: {state!.LastNotificationId}");
			if (OnStateChange != null)
				await OnStateChange.Invoke(state);
		}

    }
}

