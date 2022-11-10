using System;
using ConsensusChessShared.DTO;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

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
			=> await PostAsync($"{state.Name} ({state.Shortcode}): {status}", PostType.SocialStatus);

		public async Task<PostReport> PostAsync(Game game)
			=> await PostAsync(
				string.Format("New {0} game...\nWhite: {1}\nBlack: {2}\nMove duration: {3}\n",
					game.SideRules,
                    string.Join(", ", game.WhiteNetworks),
                    string.Join(", ", game.BlackNetworks),
                    game.MoveDuration),
                PostType.EngineUpdate);

		public async Task<PostReport> PostAsync(string text, PostType type = PostType.Unspecified)
		{
			log.LogInformation($"Posting: {text}");

			var post = new Post()
			{
				Created = DateTime.Now.ToUniversalTime(),
				Message = text,
				NodeShortcode = state.Shortcode,
                NetworkServer = network.NetworkServer,
                AppName = network.AppName,
                Type = type
            };

            return await PostToNetworkAsync(post);
		}

		public async Task<PostReport> ReplyAsync(SocialCommand origin, string message)
		{
            log.LogInformation($"Replying: {message}");

            var post = new Post()
            {
                Created = DateTime.Now.ToUniversalTime(),
                Message = message,
                NodeShortcode = state.Shortcode,
                NetworkServer = network.NetworkServer,
                AppName = network.AppName,
                Type = PostType.TextResponse,
				ReplyTo = origin.SourceId
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

