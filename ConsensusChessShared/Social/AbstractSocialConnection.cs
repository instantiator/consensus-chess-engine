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
			=> await PostAsync($"{network.Name}: {status}", PostType.SocialStatus);

		public async Task<PostReport> PostAsync(string text, PostType type = PostType.Unspecified)
		{
			log.LogInformation($"Posting: {text}");

            var post = new Post()
            {
                Created = DateTime.Now.ToUniversalTime(),
                Message = text,
                NetworkName = network.Name,
                Type = type
            };

            return await PostToNetworkAsync(post);
		}

		public abstract Task<PostReport> PostToNetworkAsync(Post post);

		protected async Task ReportStateChangeAsync()
		{
			log.LogDebug($"ReportStateChange - new notification id: {state.LastNotificationId}");
			if (OnStateChange != null)
				await OnStateChange.Invoke(state);
		}
    }
}

