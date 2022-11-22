using System;
using System.Diagnostics;
using System.Xml.Linq;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Helpers;
using ConsensusChessShared.Service;
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
        public async Task StartNewGame_PostsSentAndDbEntriesCreated()
        {
            var started = DateTime.Now;

            // confirm there are no games before we start the test
            using (var db = GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());
            }

            // issue a command to start a new game
            WriteLogLine("Posting new game request...");
            var commandNewGame = await SendMessageAsync("new node-0-test", Mastonet.Visibility.Direct, accounts["engine"]);
            await AssertFavouritedAsync(commandNewGame);

            // check there's 1 reply responding to the new game command
            WriteLogLine("Checking for reply from engine..");
            var expectedGameAck = "@instantiator New MoveLock game for: node-0-test";
            await AssertGetsReplyNotificationAsync(commandNewGame, expectedGameAck);

            // check db for the new game
            WriteLogLine("Checking for game in database...");
            using (var db = GetDb())
            {
                Assert.AreEqual(1, db.Games.Count());
                var game = db.Games.Single();

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
            WriteLogLine($"Checking for new game announcement by engine...");
            var engineStatuses = await AssertAndGetStatusesAsync(engine, 1,
                (Status status, string content) => content.Contains("New MoveLock game...") && status.CreatedAt > started);

            // check the node posted the new board
            WriteLogLine($"Checking for board post by node...");
            var nodeStatuses = await AssertAndGetStatusesAsync(node, 1,
                (Status status, string content) => content.Contains("New board.") && status.CreatedAt > started);

            WriteLogLine($"Checking for board post in db...");
            using (var db = GetDb())
            {
                Assert.AreEqual(1, db.Games.Count());
                var game = db.Games.Single();

                // check for the board post
                var boardPost = game.CurrentMove.From.BoardPosts.Single();

                WriteLogLine($"Board post id: {boardPost.NetworkPostId}");

                Assert.AreEqual("botsin.space", boardPost.NetworkServer);
                Assert.AreEqual("node-0-test", boardPost.NodeShortcode);
                Assert.IsTrue(boardPost.Message.Contains("New board."));
            }
        }

        [TestMethod]
        public async Task VoteOnGame_VoteStoredResponseSent()
        {
            var started = DateTime.Now;

            // get node account
            var node = await GetAccountAsync(accounts["node"]);

            // init the db with a test game
            using (var db = GetDb())
            {
                // confirm no games
                Assert.AreEqual(0, db.Games.Count());

                // create a game
                var servers = new[] { node.AccountName };
                var nodes = new[] { "node-0-test" };
                var game = Game.NewGame("test-game", "A game for integration testing", servers, servers, nodes, nodes, SideRules.MoveLock);
                db.Games.Add(game);
                await db.SaveChangesAsync();
            }

            // wait for the node to post the new board
            var nodeStatuses = await AssertAndGetStatusesAsync(node, 1,
                (Status status, string content) => content.Contains("New board.") && status.CreatedAt > started);
            var boardStatus = nodeStatuses.Single();

            // post a move
            var moveStatus = await SendMessageAsync("move e4", Mastonet.Visibility.Direct, node.AccountName, boardStatus.Id);

            // confirm the reply comes through
            var verifiedStatus = await AssertGetsReplyNotificationAsync(moveStatus, "@instantiator Move accepted - thank you");

            using (var db = GetDb())
            {
                // confirm no games
                Assert.AreEqual(1, db.Games.Count());

                var game = db.Games.ToList().Single();

                var votes = game.CurrentMove.Votes;
                Assert.AreEqual(1, votes.Count());

                var vote = votes.Single();
                Assert.AreEqual("instantiator@mastodon.social", vote.Participant.NetworkUserAccount);
                Assert.AreEqual("mastodon.social", vote.Participant.NetworkServer);
                Assert.AreEqual(VoteValidationState.Valid, vote.ValidationState);

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost!.Succeeded);
                Assert.IsNotNull(vote.ValidationPost.NetworkPostId);
                Assert.AreEqual("node-0-test", vote.ValidationPost.NodeShortcode);
                Assert.IsTrue(vote.ValidationPost.NetworkReplyToId > 0);
                Assert.IsTrue(vote.ValidationPost.Message.Contains("Move accepted - thank you"));

                Assert.IsTrue(vote.NetworkMovePostId > 0);
                Assert.AreEqual("e4", vote.MoveText);
            }
        }

        [TestMethod]
        public async Task VoteOnGameTwice_VoteSuperceded()
        {
            var started = DateTime.Now;

            // get node account
            var node = await GetAccountAsync(accounts["node"]);

            // init the db with a test game
            using (var db = GetDb())
            {
                // confirm no games
                Assert.AreEqual(0, db.Games.Count());

                // create a game
                var servers = new[] { node.AccountName };
                var nodes = new[] { "node-0-test" };
                var game = Game.NewGame("test-game", "A game for integration testing", servers, servers, nodes, nodes, SideRules.MoveLock);
                db.Games.Add(game);
                await db.SaveChangesAsync();
            }

            // wait for the node to post the new board
            var nodeStatuses = await AssertAndGetStatusesAsync(node, 1,
                (Status status, string content) => content.Contains("New board.") && status.CreatedAt > started);
            var boardStatus = nodeStatuses.Single();

            // post a move, wait for favourite and move on (no need to check the reply)
            var moveStatus = await SendMessageAsync("move e4", Mastonet.Visibility.Direct, node.AccountName, boardStatus.Id);
            await AssertFavouritedAsync(moveStatus);

            using (var db = GetDb())
            {
                var game = db.Games.ToList().Single();
                var participant = db.Participant.Single(p => p.NetworkUserAccount == "instantiator@mastodon.social");
                var gm = new GameManager(mockLogger.Object);
                var preexistingVote = gm.GetCurrentValidVote(game.CurrentMove, participant);

                Assert.IsNotNull(preexistingVote);
                Assert.AreEqual("e4", preexistingVote.MoveText);
                Assert.AreEqual(VoteValidationState.Valid, preexistingVote.ValidationState);
            }

            // post a 2nd move
            var moveStatus2 = await SendMessageAsync("move e3", Mastonet.Visibility.Direct, node.AccountName, boardStatus.Id);
            await AssertFavouritedAsync(moveStatus2);
            // confirm the reply comes through
            var verifiedStatus2 = await AssertGetsReplyNotificationAsync(moveStatus2, "@instantiator Move accepted - thank you");

            // check db, and vote statuses
            using (var db = GetDb())
            {
                // confirm no games
                Assert.AreEqual(1, db.Games.Count());

                var game = db.Games.ToList().Single();

                var votes = game.CurrentMove.Votes;
                Assert.AreEqual(2, votes.Count());

                Assert.AreEqual(1, votes.Count(v => v.ValidationState == VoteValidationState.Superceded));
                Assert.AreEqual(1, votes.Count(v => v.ValidationState == VoteValidationState.Valid));
            }
        }
    }
}
