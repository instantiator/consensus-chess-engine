using System;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using Mastonet.Entities;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public abstract class AbstractSocialConnection : ISocialConnection
	{
		public event Func<NodeState, Task> OnStateChange;

		protected ILogger log;
		protected Network network;
		protected NodeState? state;
		protected bool dryRuns;

        protected event Func<SocialCommand, Task>? asyncCommandReceivers;
        protected IEnumerable<SocialCommand>? missedCommands;

        public AbstractSocialConnection(ILogger log, Network network)
		{
			this.log = log;
			this.network = network;
			this.dryRuns = network.DryRuns;
		}

		public async Task InitAsync(NodeState state)
        {
            this.state = state;
            await InitImplementationAsync();
        }

        protected abstract Task InitImplementationAsync();

		public abstract string? DisplayName { get; }
		public abstract string? AccountName { get; }
		public abstract IEnumerable<string> CalculateCommandSkips();
        protected abstract Task GetMissedCommands();
        protected abstract Task MarkCommandProcessedAsync(long id);
        protected abstract Task StartListeningForNotificationsAsync();
        protected abstract Task<IEnumerable<Notification>> GetAllNotificationSinceAsync(long sinceId);
        public abstract Task StopListeningForCommandsAsync(Func<SocialCommand, Task> asyncReceiver);
        public abstract Task<Post> PostToNetworkAsync(Post post, bool dryRun);

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
                if (command.IsForThisNode && command.SourceId != null && !command.IsProcessed)
                {
                    await MarkCommandProcessedAsync(command.SourceId.Value);
                    command.IsProcessed = true;
                }

                // always update the last notification id
                if (command.SourceId.HasValue && command.SourceId.Value > state!.LastNotificationId)
                {
                    state.LastNotificationId = command.SourceId.Value;
                    await ReportStateChangeAsync();
                }
            }
        }

		public async Task<Post> PostAsync(SocialStatus status, bool? dryRun = null)
			=> await PostAsync($"{state.Name} ({state.Shortcode}): {status}", PostType.SocialStatus, dryRun);

		public async Task<Post> PostAsync(Game game, bool? dryRun = null)
			=> await PostAsync(
				string.Format("New {0} game...\nWhite: {1}\nBlack: {2}\nMove duration: {3}",
					game.SideRules,
                    string.Join(", ", game.WhiteParticipantNetworkServers),
                    string.Join(", ", game.BlackParticipantNetworkServers),
                    game.MoveDuration),
                PostType.GameAnnouncement,
                dryRun);

        public async Task<Post> PostAsync(Game game, Board board, bool? dryRun = null)
            => await PostAsync(
                string.Format("New board. You have {1} to vote.\n{0}",
                    BoardFormatter.VisualiseEmoji(board),
					game.MoveDuration.ToString()),
                PostType.BoardUpdate,
				dryRun);

        public async Task<Post> PostAsync(string text, PostType type = PostType.Unspecified, bool? dryRun = null)
		{
			log.LogInformation($"Posting: {text}");

			var post = new Post()
			{
				Message = text,
				NodeShortcode = state!.Shortcode,
                NetworkServer = network.NetworkServer,
                AppName = network.AppName,
                Type = type
            };

            return await PostToNetworkAsync(post, dryRun ?? dryRuns);
		}

		public async Task<Post> ReplyAsync(SocialCommand origin, string message, PostType? postType = null, bool? dryRun = null)
		{
            // prepend username to reply
            message = $"@{origin.NetworkUserId} {message}";

            log.LogInformation($"Replying: {message}");

            var post = new Post()
            {
                Message = message,
                NodeShortcode = state!.Shortcode,
                NetworkServer = network.NetworkServer,
                AppName = network.AppName,
                Type = postType ?? PostType.Unspecified,
				NetworkReplyToId = origin.SourceId,
            };

            return await PostToNetworkAsync(post, dryRun ?? dryRuns);
        }

		protected async Task ReportStateChangeAsync()
		{
			log.LogDebug($"ReportStateChange - new notification id: {state!.LastNotificationId}");
			if (OnStateChange != null)
				await OnStateChange.Invoke(state);
		}

    }
}

