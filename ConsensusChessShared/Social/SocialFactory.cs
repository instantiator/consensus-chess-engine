using System;
using ConsensusChessShared.DTO;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public class SocialFactory
	{
		public static ISocialConnection From(ILogger log, Network network, NodeState state, bool dryRuns)
		{
			switch (network.Type)
			{
				case NetworkType.Mastodon:
					return new MastodonConnection(log, network, state, dryRuns);

				default:
					throw new ArgumentException($"Network {network.Type} unknown.");
			}
		}

	}
}

