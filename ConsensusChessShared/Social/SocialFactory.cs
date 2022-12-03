using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public class SocialFactory
	{
		public static ISocialConnection From(ILogger log, Network network, string shortcode, ServiceConfig config)
		{
			switch (network.Type)
			{
				case NetworkType.Mastodon:
					return new MastodonConnection(log, network, shortcode, config);

				default:
					throw new ArgumentException($"Network {network.Type} unknown.");
			}
		}

	}
}

