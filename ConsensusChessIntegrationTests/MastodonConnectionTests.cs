using System;
using System.Diagnostics;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Mastonet;
using Mastonet.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;

namespace ConsensusChessIntegrationTests
{
	[TestClass]
	public class MastodonConnectionTests
	{
        private Network GetNetwork()
			=> Network.FromEnv(Environment.GetEnvironmentVariables());

        private int MAX_PAGES = 6;
        private List<SocialCommand> receivedCommands = new List<SocialCommand>();

        [TestInitialize]
        public void Reset()
        {
            receivedCommands.Clear();
        }

        [TestMethod]
        public async Task MastodonClient_StartsListening()
        {
            var mockLogger = new Mock<ILogger>();
            var network = GetNetwork();
            var config = new ServiceConfig("#ConsensusChessIntegrationTests", "instantiator@mastodon.social", Mastonet.Visibility.Unlisted);
            var connection = new MoreExposedMastodonConnection(mockLogger.Object, network, "personal-test", config);
            var state = new NodeState("personal test connection", "personal-test", network);
            await connection.InitAsync(state);
            Assert.IsTrue(connection.Ready);
            await connection.StartListeningForCommandsAsync(CommandReceiver, true);
            Assert.IsTrue(connection.Streaming);
        }

        public async Task CommandReceiver(SocialCommand cmd)
        {
            receivedCommands.Add(cmd);
        }

        [TestMethod]
        public async Task MastodonClient_PaginatesIndefinitely()
        {
            // the test actually caps pagination at MAX_PAGES, but this proves it could go on...
			var network = GetNetwork();
            var client = new MastodonClient(network.NetworkServer, network.AppToken);
            var notifications = await GetAllNotificationsSinceSinceIdAsync(client, 0, MAX_PAGES);
            Assert.IsNotNull(notifications);
            Assert.IsTrue(notifications.Count() > 2);
            Assert.AreEqual(MAX_PAGES, recentIterations);
        }

        [TestMethod]
		public async Task MastodonConnection_PaginationRespectsSinceId()
		{
            var mockLogger = new Mock<ILogger>();
            var network = GetNetwork();
            var config = new ServiceConfig("#ConsensusChessIntegrationTests", "instantiator@mastodon.social", Mastonet.Visibility.Unlisted);
			var connection = new MoreExposedMastodonConnection(mockLogger.Object, network, "personal-test", config);
			var state = new NodeState("personal test connection", "personal-test", network);
			await connection.InitAsync(state);

			var debug = new List<String>();

			var notifications = await connection.ExposeGetAllNotificationSinceAsync(null);
			Assert.IsNotNull(notifications);
			Assert.IsTrue(notifications.Count() > 2);
			Assert.AreEqual(MastodonConnection.MAX_PAGES, connection.RecentIterations, $"Iterated {connection.RecentIterations} times.");

            debug.Add($"notifications.Count() = {notifications.Count()}");
            debug.Add($"minId = {notifications.Min(n => long.Parse(n.Id))}");

            // find an id in the middle of the pack
            var sinceId = FindAMiddleishId(notifications);
            debug.Add($"sinceId = {sinceId}");

            var justSecondHalf = await connection.ExposeGetAllNotificationSinceAsync(sinceId);
            debug.Add($"justSecondHalf.Count() = {justSecondHalf.Count()}");

			Assert.IsTrue(justSecondHalf.Count() > 0);
			Assert.IsTrue(justSecondHalf.Count() < notifications.Count(), string.Join("\n", debug));
		}

        private string FindAMiddleishId(IEnumerable<Notification> notifications)
        {
            var avgTicks = (notifications.Max(n => n.CreatedAt.Ticks) + notifications.Min(n => n.CreatedAt.Ticks)) / 2;
            var closestDiff = notifications.Min(n => Math.Abs(n.CreatedAt.Ticks - avgTicks));
            var midNotification = notifications.Single(n => Math.Abs(n.CreatedAt.Ticks - avgTicks) == closestDiff);
            return midNotification.Id;
        }

        private int recentIterations;
        private async Task<IEnumerable<Notification>> GetAllNotificationsSinceSinceIdAsync(MastodonClient client, long sinceId, int maxPages)
        {
            var list = new List<Notification>();
            long? nextPageMaxId = null;
            recentIterations = 0;

            do
            {
                ArrayOptions opts = new ArrayOptions()
                {
                    SinceId = sinceId.ToString(),
                    MaxId = nextPageMaxId?.ToString()
                };

                var page = await client.GetNotifications(options: opts);

                list.AddRange(page.Where(pn => !list.Select(n => n.Id).Contains(pn.Id)));
                nextPageMaxId = page.NextPageMaxId;
                recentIterations++;
            } while (nextPageMaxId != null && recentIterations < maxPages);

            return list;
        }

    }
}

