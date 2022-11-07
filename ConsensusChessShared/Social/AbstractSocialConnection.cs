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
        public abstract Task<PostReport> PostStatusAsync(SocialStatus status);
        public abstract Task<PostReport> PostStatusAsync(string detail);
        public abstract void StartListening(Action<SocialCommand> receiver, DateTime? backdate);
        public abstract void StopListening(Action<SocialCommand> receiver);
    }
}

