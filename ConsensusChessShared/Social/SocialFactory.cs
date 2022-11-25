using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public class SocialFactory
	{
		public static ISocialConnection From(ILogger log, Network network, string shortcode)
		{
			switch (network.Type)
			{
				case NetworkType.Mastodon:
					return new MastodonConnection(log, network, shortcode);

				default:
					throw new ArgumentException($"Network {network.Type} unknown.");
			}
		}

	}
}

