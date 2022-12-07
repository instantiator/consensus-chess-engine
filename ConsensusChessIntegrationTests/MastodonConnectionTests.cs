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
            var ignorables = new string[] { "#hush" };
            var config = new ServiceConfig("#ConsensusChessIntegrationTests", "instantiator@mastodon.social", Mastonet.Visibility.Unlisted, ignorables, true);
            var connection = new MoreExposedMastodonConnection(mockLogger.Object, network, "personal-test", config);
            var state = new NodeState("personal test connection", "personal-test", network);
            await connection.InitAsync(state);
            Assert.IsTrue(connection.Ready);
            await connection.StartListeningForCommandsAsync(CommandReceiver, true);
            Assert.IsTrue(connection.Streaming);
        }

        private async Task CommandReceiver(SocialCommand cmd)
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
            var ignorables = new string[] { "#hush" };
            var config = new ServiceConfig("#ConsensusChessIntegrationTests", "instantiator@mastodon.social", Mastonet.Visibility.Unlisted, ignorables, true);
			var connection = new MoreExposedMastodonConnection(mockLogger.Object, network, "personal-test", config);
			var state = new NodeState("personal test connection", "personal-test", network);
			await connection.InitAsync(state);

			var debug = new List<String>();

			var commands = await connection.ExposeGetAllNotificationSinceAsync(false, null);
			Assert.IsNotNull(commands);
			Assert.IsTrue(commands.Count() > 2);
			Assert.AreEqual(MastodonConnection.MAX_PAGES, connection.RecentIterations, $"Iterated {connection.RecentIterations} times.");

            debug.Add($"notifications.Count() = {commands.Count()}");
            debug.Add($"minId = {commands.Min(n => long.Parse(n.SourceNotificationId!))}");

            // find an id in the middle of the pack
            var sinceId = FindAMiddleishId(commands);
            debug.Add($"sinceId = {sinceId}");

            var justSecondHalf = await connection.ExposeGetAllNotificationSinceAsync(false, sinceId);
            debug.Add($"justSecondHalf.Count() = {justSecondHalf.Count()}");

			Assert.IsTrue(justSecondHalf.Count() > 0);
			Assert.IsTrue(justSecondHalf.Count() < commands.Count(), string.Join("\n", debug));
		}

        private string FindAMiddleishId(IEnumerable<SocialCommand> commands)
        {
            var avgTicks = (commands.Max(c => c.SourceCreated.Ticks) + commands.Min(c => c.SourceCreated.Ticks)) / 2;
            var closestDiff = commands.Min(n => Math.Abs(n.SourceCreated.Ticks - avgTicks));
            var midNotification = commands.Single(n => Math.Abs(n.SourceCreated.Ticks - avgTicks) == closestDiff);
            return midNotification.SourceNotificationId!;
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

