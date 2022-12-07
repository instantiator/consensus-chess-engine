using System;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Mastonet.Entities;
using Microsoft.Extensions.Logging;

namespace ConsensusChessIntegrationTests
{
    public class MoreExposedMastodonConnection : MastodonConnection
    {
        public MoreExposedMastodonConnection(ILogger log, Network network, string shortcode, ServiceConfig config)
            : base(log, network, shortcode, config)
        {
            MAX_PAGES = 6;
        }

        public async Task<IEnumerable<SocialCommand>> ExposeGetAllNotificationSinceAsync(bool isRetrospective, string? sinceId)
            => await GetAllNotificationSinceAsync(isRetrospective, sinceId);

        public int RecentIterations => recentIterations;
        
    }
}

