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

        public abstract Task<string> GetDisplayNameAsync();
        public abstract void StartListening(Action<SocialCommand> receiver, DateTime? backdate);
        public abstract void StopListening(Action<SocialCommand> receiver);

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

