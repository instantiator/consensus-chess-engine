using System;
namespace ConsensusChessIntegrationTests
{
	[TestClass]
	public class SimpleMessageTests : AbstractIntegrationTests
	{
		[Ignore]
		[TestMethod]
		public async Task SendAMessageToEngine_ItIsFavourited()
		{

			var status = await SendMessageAsync("hello", Mastonet.Visibility.Direct, "@icgames-engine@botsin.space");
			var notification = await AwaitNotification(TimeSpan.FromSeconds(30), (n) => n.Type == "favourite");

			Assert.IsNotNull(status);
			Assert.IsNotNull(notification);
            Assert.IsNotNull(notification.Status);
            Assert.AreEqual(notification.Status.Id, status.Id);
		}

	}
}

