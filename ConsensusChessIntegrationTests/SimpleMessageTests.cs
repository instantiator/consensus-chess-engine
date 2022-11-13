using System;
namespace ConsensusChessIntegrationTests
{
	[TestClass]
	public class SimpleMessageTests : AbstractIntegrationTests
	{
		[TestMethod]
		public async Task SendAMessageToEngine_ItIsFavourited()
		{

			var status = await SendMessageAsync("hello", Mastonet.Visibility.Direct, accounts["engine"]);
			var notification = await AwaitNotification(
				TimeSpan.FromSeconds(60),
				(n) => n.Type == "favourite" && n.Status.Id == status.Id);

			Assert.IsNotNull(status);
			Assert.IsNotNull(notification);
            Assert.IsNotNull(notification.Status);
            Assert.AreEqual(notification.Status.Id, status.Id);
		}

	}
}

