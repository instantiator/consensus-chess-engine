using System;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConsensusChessIntegrationTests
{
	[TestClass]
	public class MastodonConnectionTests
	{
        private Network GetNetwork()
			=> Network.FromEnv(Environment.GetEnvironmentVariables());

        [TestMethod]
		public async Task PagingWorksABit()
		{
            var mockLogger = new Mock<ILogger>();
            var network = GetNetwork();

            var config = new ServiceConfig("#ConsensusChessIntegrationTests", "instantiator@mastodon.social", Mastonet.Visibility.Unlisted);
			var connection = new MoreExposedMastodonConnection(mockLogger.Object, network, "personal-test", config);
			var state = new NodeState("personal test connection", "personal-test", network);
			await connection.InitAsync(state);

			var items = new List<String>();

			var notifications = await connection.ExposeGetAllNotificationSinceAsync(0);
			Assert.IsNotNull(notifications);
			Assert.IsTrue(notifications.Count() > 2);
			//Assert.AreEqual(MastodonConnection.MAX_PAGES-1, connection.RecentIterations, $"Iterated {connection.RecentIterations} times.");

			items.Add($"notifications.Count() = {notifications.Count()}");
            items.Add($"minId = {notifications.Min(n => long.Parse(n.Id))}");

            var avgTicks = (notifications.Max(n => n.CreatedAt.Ticks) + notifications.Min(n => n.CreatedAt.Ticks)) / 2;
			var closestDiff = notifications.Min(n => Math.Abs(n.CreatedAt.Ticks - avgTicks));
			var midNotification = notifications.Single(n => Math.Abs(n.CreatedAt.Ticks - avgTicks) == closestDiff);
			var midId = long.Parse(midNotification.Id);
            items.Add($"midId = {midId}");

            var justSecondHalf = await connection.ExposeGetAllNotificationSinceAsync(midId);
            items.Add($"justSecondHalf.Count() = {justSecondHalf.Count()}");

			Assert.IsTrue(justSecondHalf.Count() > 0);
			Assert.IsTrue(justSecondHalf.Count() < notifications.Count(), string.Join("\n", items));
		}
	}
}

