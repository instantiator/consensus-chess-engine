using System;
using ConsensusChessShared.DTO;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public abstract class AbstractSocialConnection : ISocialConnection
	{
		public event Func<NodeState, Task> OnStateChange;

		protected ILogger log;
		protected Network network;
		protected NodeState state;

		public AbstractSocialConnection(ILogger log, Network network, NodeState state)
		{
			this.log = log;
			this.network = network;
			this.state = state;
		}

		public abstract Task InitAsync();
		public abstract string DisplayName { get; }
		public abstract string AccountName { get; }
		public abstract IEnumerable<string> CalculateCommandSkips();
		public abstract Task StartListeningForCommandsAsync(Func<SocialCommand, Task> asyncReceiver, bool retrieveMissedCommands);
		public abstract Task StopListeningForCommandsAsync(Func<SocialCommand, Task> asyncReceiver);

		public async Task<PostReport> PostAsync(SocialStatus status)
			=> await PostAsync($"{network.Name}: {status}");

		public async Task<PostReport> PostAsync(string text)
		{
			log.LogInformation($"Posting: {text}");
			return await PostToNetworkAsync(text);
		}

		protected abstract Task<PostReport> PostToNetworkAsync(string detail);

		protected async Task ReportStateChangeAsync()
		{
			log.LogDebug($"ReportStateChange - new notification id: {state.LastNotificationId}");
			if (OnStateChange != null)
				await OnStateChange.Invoke(state);
		}
    }
}

