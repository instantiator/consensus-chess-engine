using System;
using ConsensusChessShared.DTO;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public abstract class AbstractSocialConnection : ISocialConnection
	{
		protected ILogger log;
		protected Network network;

		public AbstractSocialConnection(ILogger log, Network network)
		{
			this.log = log;
			this.network = network;
		}

		public abstract Task InitAsync();
		public abstract string DisplayName { get; }
        public abstract string AccountName { get; }
        public abstract IEnumerable<string> CalculateCommandSkips();
        public abstract Task StartListeningForCommandsAsync(Func<SocialCommand, Task> asyncReceiver, long? sinceId);
        public abstract Task StopListeningForCommandsAsync(Func<SocialCommand, Task> asyncReceiver);

        public async Task<PostReport> PostAsync(SocialStatus status)
			=> await PostAsync($"{network.Name}: {status}");

		public async Task<PostReport> PostAsync(string text)
		{
			log.LogInformation($"Posting: {text}");
			return await PostToNetworkAsync(text);
		}

        protected abstract Task<PostReport> PostToNetworkAsync(string detail);
    }
}

