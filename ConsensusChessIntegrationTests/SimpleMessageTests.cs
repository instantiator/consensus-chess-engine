using System;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Helpers;
using Mastonet.Entities;

namespace ConsensusChessIntegrationTests
{
	[TestClass]
	public class SimpleMessageTests : AbstractIntegrationTests
	{
        public const int TIMEOUT_mins = 10;

		[TestMethod]
		public async Task SendAMessageToEngine_ItIsFavourited()
		{
			var status = await SendMessageAsync("hello", Mastonet.Visibility.Direct, accounts["engine"]);
            Assert.IsNotNull(status);

            var notifications = await AwaitNotifications(
				TimeSpan.FromMinutes(TIMEOUT_mins),
				(n) => n.Type == "favourite" && n.Status != null && n.Status.Id == status.Id);

            Assert.IsNotNull(notifications.Single().Status);
            Assert.AreEqual(notifications.Single().Status!.Id, status.Id);
		}

        [TestMethod]
		public async Task CommandNew_StartsNewGame()
		{
            // confirm there are no games before we start the test
            using (var db = GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());
            }

            // issue a command to start a new game
            var commandNewGame = await SendMessageAsync("new node-0-test", Mastonet.Visibility.Direct, accounts["engine"]);
            Assert.IsNotNull(commandNewGame);

            // check it was favourited
            var notification_favourite = await AwaitNotifications(
                TimeSpan.FromMinutes(TIMEOUT_mins),
                (n) => n.Type == "favourite" && n.Status != null && n.Status.Id == commandNewGame.Id);

            Assert.IsNotNull(notification_favourite.Single().Status);
            Assert.AreEqual(notification_favourite.Single().Status!.Id, commandNewGame.Id);

            // check there's a reply
            var notification_replyNewGame = await AwaitNotifications(
                TimeSpan.FromMinutes(TIMEOUT_mins),
                (n) => n.Type == "mention" && n.Status != null && n.Status.InReplyToId == commandNewGame.Id);

            // I _think_ the reply mention is shortened on delivery to the receiving server?
            var expectedGameAck = "@instantiator New MoveLock game for: node-0-test";
            var receivedGameAck = CommandHelper.RemoveUnwantedTags(notification_replyNewGame.Single().Status!.Content);
            Assert.AreEqual(expectedGameAck, receivedGameAck);

            // check db for the new game
            using (var db = GetDb())
            {
                Assert.AreEqual(1, db.Games.Count());
                var game = db.Games.First();

                Assert.AreEqual(1, game.WhitePostingNodeShortcodes.Count());
                Assert.AreEqual(1, game.BlackPostingNodeShortcodes.Count());

                Assert.AreEqual(SideRules.MoveLock, game.SideRules);

                Assert.AreEqual("node-0-test", game.WhitePostingNodeShortcodes.First());
                Assert.AreEqual("node-0-test", game.BlackPostingNodeShortcodes.First());

                Assert.AreEqual(1, game.WhiteParticipantNetworkServers.Count());
                Assert.AreEqual(1, game.BlackParticipantNetworkServers.Count());

                Assert.AreEqual("botsin.space", game.WhiteParticipantNetworkServers.First());
                Assert.AreEqual("botsin.space", game.BlackParticipantNetworkServers.First());
            }
        }
    }
}

