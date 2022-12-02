using System;
using System.Diagnostics;
using System.Xml.Linq;
using ConsensusChessIntegrationTests.Data;
using ConsensusChessShared.Constants;
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
            var status = await SendMessageAsync("hello", Mastonet.Visibility.Direct, contacts[NodeType.Engine]);
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

            var nodeShortcodes = new[] { contacts[NodeType.Node].Shortcode! };

            // issue a command to start a new game
            WriteLogLine("Posting new game request...");
            var commandNewGame = await SendMessageAsync(
                Messages.NewGameCommand(nodeShortcodes),
                Mastonet.Visibility.Direct,
                contacts[NodeType.Engine]);

            await AssertFavouritedAsync(commandNewGame);

            // check there's 1 reply responding to the new game command
            WriteLogLine("Checking for reply from engine...");
            await AssertGetsReplyNotificationAsync(
                commandNewGame,
                Responses.NewGame_reply(SideRules.MoveLock, nodeShortcodes));

            // check db for the new game
            WriteLogLine("Checking for game in database...");
            using (var db = GetDb())
            {
                Assert.AreEqual(1, db.Games.Count());
                var game = db.Games.Single();

                Assert.AreEqual(1, game.WhitePostingNodeShortcodes.Count());
                Assert.AreEqual(1, game.BlackPostingNodeShortcodes.Count());

                Assert.AreEqual(SideRules.MoveLock, game.SideRules);

                Assert.AreEqual(contacts[NodeType.Node].Shortcode, game.WhitePostingNodeShortcodes.First().Value);
                Assert.AreEqual(contacts[NodeType.Node].Shortcode, game.BlackPostingNodeShortcodes.First().Value);

                Assert.AreEqual(1, game.WhiteParticipantNetworkServers.Count());
                Assert.AreEqual(1, game.BlackParticipantNetworkServers.Count());

                Assert.AreEqual(contacts[NodeType.Node].Server, game.WhiteParticipantNetworkServers.First().Value);
                Assert.AreEqual(contacts[NodeType.Node].Server, game.BlackParticipantNetworkServers.First().Value);
            }

            // get node and engine accounts
            var nodeAcct = await GetAccountAsync(contacts[NodeType.Node]);
            var engineAcct = await GetAccountAsync(contacts[NodeType.Engine]);

            // check the engine posted the new board
            WriteLogLine($"Checking for new game announcement by engine...");
            var engineStatuses = await AssertAndGetStatusesAsync(engineAcct, 1,
                (Status status, string content)
                    => content.Contains(Responses.NewGame_announcement(SideRules.MoveLock))
                    && status.CreatedAt > started);

            // check the node posted the new board
            WriteLogLine($"Checking for board post by node...");
            var nodeStatuses = await AssertAndGetStatusesAsync(nodeAcct, 1,
                (Status status, string content) =>
                    content.ToLower().Contains(Responses.NewBoard())
                    && status.CreatedAt > started);

            WriteLogLine($"Checking for board post in db...");
            using (var db = GetDb())
            {
                Assert.AreEqual(1, db.Games.Count());
                var game = db.Games.Single();

                // check for the board post
                var boardPost = game.CurrentMove.From.BoardPosts.Single();

                WriteLogLine($"Board post id: {boardPost.NetworkPostId}");

                Assert.AreEqual(contacts[NodeType.Node].Server, boardPost.NetworkServer);
                Assert.AreEqual(contacts[NodeType.Node].Shortcode, boardPost.NodeShortcode);
                Assert.IsTrue(boardPost.Message!.ToLower().Contains(Responses.NewBoard()));
            }
        }

        [TestMethod]
        public async Task VoteOnGame_VoteStoredResponseSent()
        {
            var started = DateTime.Now;

            // get node account
            var node = await GetAccountAsync(contacts[NodeType.Node]);

            // init the db with a test game
            using (var db = GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());

                // create a game
                var servers = new[] { contacts[NodeType.Node].Server };
                var nodes = new[] { contacts[NodeType.Node].Shortcode! };
                var game = new Game(
                    "test-game", "Int Game", "A game for integration testing",
                    servers, servers,
                    nodes, nodes,
                    SideRules.MoveLock);

                db.Games.Add(game);
                await db.SaveChangesAsync();
            }

            // wait for the node to post the new board
            var nodeStatuses = await AssertAndGetStatusesAsync(node, 1,
                (Status status, string content)
                    => content.ToLower().Contains(Responses.NewBoard())
                    && status.CreatedAt > started);

            var boardStatus = nodeStatuses.Single();

            // post a move
            var moveStatus = await SendMessageAsync(
                Messages.Move("e2","e4"),
                Mastonet.Visibility.Direct,
                contacts[NodeType.Node],
                long.Parse(boardStatus.Id));

            // confirm the reply comes through
            var verifiedStatus = await AssertGetsReplyNotificationAsync(moveStatus, Responses.MoveAccepted());

            using (var db = GetDb())
            {
                Assert.AreEqual(1, db.Games.Count());
                var game = db.Games.ToList().Single();

                var votes = game.CurrentMove.Votes;
                Assert.AreEqual(1, votes.Count());

                var vote = votes.Single();
                Assert.AreEqual(username.Full, vote.Participant.Username.Full);
                Assert.AreEqual(username.Server, vote.Participant.Username.Server);
                Assert.AreEqual(VoteValidationState.Valid, vote.ValidationState);

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost!.Succeeded);
                Assert.IsNotNull(vote.ValidationPost.NetworkPostId);
                Assert.AreEqual(contacts[NodeType.Node].Shortcode, vote.ValidationPost.NodeShortcode);
                Assert.IsTrue(vote.ValidationPost.NetworkReplyToId > 0);
                Assert.IsTrue(vote.ValidationPost.Message.Contains(Responses.MoveAccepted()));

                Assert.IsTrue(vote.NetworkMovePostId > 0);
                Assert.AreEqual("e2 - e4", vote.MoveText);
                Assert.AreEqual("e4", vote.MoveSAN);
            }
        }

    }
}
