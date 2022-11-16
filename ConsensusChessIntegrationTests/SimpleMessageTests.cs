using System;
using System.Diagnostics;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Helpers;
using Mastonet.Entities;

namespace ConsensusChessIntegrationTests
{
    [TestClass]
    public class SimpleMessageTests : AbstractIntegrationTests
    {
        [TestMethod]
        public async Task SendAMessageToEngine_ItIsFavourited()
        {
            var status = await SendMessageAsync("hello", Mastonet.Visibility.Direct, accounts["engine"]);
            await AssertFavouritedAsync(status);
        }

        [TestMethod]
        public async Task CommandNew_StartsNewGame()
        {
            var started = DateTime.Now;

            // confirm there are no games before we start the test
            using (var db = GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());
            }

            // issue a command to start a new game
            var commandNewGame = await SendMessageAsync("new node-0-test", Mastonet.Visibility.Direct, accounts["engine"]);
            await AssertFavouritedAsync(commandNewGame);

            // check there's 1 reply responding to the new game command
            var expectedGameAck = "@instantiator New MoveLock game for: node-0-test";
            await AssertGetsReplyNotificationAsync(commandNewGame, expectedGameAck);

            // check db for the new game
            WriteLogLine("Checking for game in database...");
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

            // get node and engine accounts
            var node = await GetAccountAsync(accounts["node"]);
            var engine = await GetAccountAsync(accounts["engine"]);

            // check the engine posted the new board
            var engineStatuses = await AssertAndGetStatusesAsync(engine, 1,
                (Status status, string content) => content.Contains("New MoveLock game...") && status.CreatedAt > started);

            // check the node posted the new board
            var nodeStatuses = await AssertAndGetStatusesAsync(node, 1,
                (Status status, string content) => content.Contains("New board.") && status.CreatedAt > started);
        }

        [Ignore]
        [TestMethod]
        public async Task CommandMove_VotesOnNewGame()
        {
            var started = DateTime.Now;

            // confirm there are no games before we start the test
            using (var db = GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());
            }

            // issue a command to start a new game
            var commandNewGame = await SendMessageAsync("new node-0-test", Mastonet.Visibility.Direct, accounts["engine"]);
            await AssertFavouritedAsync(commandNewGame);

            // check there's 1 reply responding to the new game command
            var expectedGameAck = "@instantiator New MoveLock game for: node-0-test";
            await AssertGetsReplyNotificationAsync(commandNewGame, expectedGameAck);


        }
    }
}
