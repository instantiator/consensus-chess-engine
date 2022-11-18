using System;
using ConsensusChessShared.DTO;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public class SocialFactory
	{
		public static ISocialConnection From(ILogger log, Network network)
		{
			switch (network.Type)
			{
				case NetworkType.Mastodon:
					return new MastodonConnection(log, network);

				default:
					throw new ArgumentException($"Network {network.Type} unknown.");
			}
		}

	}
}

