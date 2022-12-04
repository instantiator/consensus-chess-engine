using System;
using ConsensusChessFeatureTests.Data;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;
using Moq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ConsensusChessFeatureTests
{
    [TestClass]
    public class EngineServiceTests : AbstractFeatureTest
    {
        [TestMethod]
        public async Task GarbageIn_GarbageOut()
        {
            var engine = await StartEngineAsync();

            var command = FeatureDataGenerator.GenerateCommand("hello", EngineNetwork);

            await receivers[EngineId.Shortcode].Invoke(command);

            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.CommandRejection &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);
        }

        [TestMethod]
        public async Task NewGameCommand_initiates_NewGame()
        {
            var engine = await StartEngineAsync();
            var node = await StartNodeAsync();

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());
            }

            var command = await SendToEngineAsync($"new {NodeId.Shortcode}");

            // acknowledgement
            EngineSocialMock.Verify(ns => ns.PostAsync(
            It.Is<Post>(p =>
                p.Succeeded == true &&
                p.Type == PostType.Engine_GameCreationResponse &&
                p.NetworkReplyToId == command.SourcePostId),
            null),
            Times.Once);

            // announcement
            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Engine_GameAnnouncement),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(1, db.Games.Count());
                Assert.IsTrue(db.Games.Single().Active);

                var game = db.Games.Single();
                Assert.AreEqual(1, game.BlackParticipantNetworkServers.Count());
                Assert.AreEqual(NodeNetwork.NetworkServer, game.BlackParticipantNetworkServers.Single().Value);
                Assert.AreEqual(1, game.WhiteParticipantNetworkServers.Count());
                Assert.AreEqual(NodeNetwork.NetworkServer, game.WhiteParticipantNetworkServers.Single().Value);

                Assert.AreEqual(1, game.BlackPostingNodeShortcodes.Count());
                Assert.AreEqual(NodeId.Shortcode, game.BlackPostingNodeShortcodes.Single().Value);
                Assert.AreEqual(1, game.WhitePostingNodeShortcodes.Count());
                Assert.AreEqual(NodeId.Shortcode, game.BlackPostingNodeShortcodes.Single().Value);

                Assert.AreEqual(Board.INITIAL_FEN, game.Moves.Single().From.FEN);
                Assert.AreEqual(1, game.GamePosts.Count(p => p.Type == PostType.Engine_GameAnnouncement));

                Assert.AreEqual(GameState.InProgress, game.State);
            }
        }

        [TestMethod]
        public async Task UnauthorisedNewGameCommand_responds_Rejection()
        {
            var engine = await StartEngineAsync();
            var node = await StartNodeAsync();

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());
            }

            var command = await SendToEngineAsync($"new {NodeId.Shortcode}", authorised: false);

            EngineSocialMock.Verify(ns => ns.PostAsync(
            It.Is<Post>(p =>
                p.Succeeded == true &&
                p.Type == PostType.CommandRejection &&
                p.NetworkReplyToId == command.SourcePostId),
            null),
            Times.Once);

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());
            }
        }

        [TestMethod]
        public async Task UnknownNodesInNewGameCommand_responds_Rejection()
        {
            var engine = await StartEngineAsync();
            var node = await StartNodeAsync();

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());
            }

            var command = await SendToEngineAsync("new beans-on-toast");

            EngineSocialMock.Verify(ns => ns.PostAsync(
            It.Is<Post>(p =>
                p.Succeeded == true &&
                p.Type == PostType.CommandRejection &&
                p.NetworkReplyToId == command.SourcePostId),
            null),
            Times.Once);

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());
            }
        }

        [TestMethod]
        public async Task GameRolloverWithMoves_resultsIn_NextMoveAndBoard()
        {
            var engine = await StartEngineAsync();
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            var boardPost = WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            // add some votes: 5 for e4, 3 for e3
            for (var i = 0; i < 5; i++)
            {
                await ReplyToNodeAsync(boardPost, $"move e2 - e4", from: $"voter-{i}");
            }
            for (var i = 5; i < 8; i++)
            {
                await ReplyToNodeAsync(boardPost, $"move e2 - e3", from: $"voter-{i}");
            }

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(8, db.Games.Single().CurrentMove.Votes.Count());
                Assert.IsTrue(db.Games.Single().CurrentMove.Votes.All(v => v.ValidationPost != null));
                Assert.IsTrue(db.Games.Single().CurrentMove.Votes.All(v => v.ValidationState == VoteValidationState.Valid));
                Assert.AreEqual(5, db.Games.Single().CurrentMove.Votes.Count(v => v.MoveText == "e2 - e4"));
                Assert.AreEqual(5, db.Games.Single().CurrentMove.Votes.Count(v => v.MoveSAN == "e4"));
                Assert.AreEqual(3, db.Games.Single().CurrentMove.Votes.Count(v => v.MoveText == "e2 - e3"));
                Assert.AreEqual(3, db.Games.Single().CurrentMove.Votes.Count(v => v.MoveSAN == "e3"));
            }

            await ExpireCurrentMoveShortlyAsync(game);
            WaitAndAssert_Moves(game, moves: 2, made: 1);
            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(2, db.Games.Single().Moves.Count());
                Assert.IsNotNull(db.Games.Single().Moves[0].To);
                Assert.AreNotEqual(db.Games.Single().Moves[0].From.FEN, db.Games.Single().Moves[0].To!.FEN);
                Assert.AreEqual(db.Games.Single().Moves[0].To!.FEN, db.Games.Single().Moves[1].From.FEN);
                Assert.AreEqual(Side.White, db.Games.Single().Moves[0].SideToPlay);
                Assert.AreEqual(Side.Black, db.Games.Single().Moves[1].SideToPlay);
                Assert.IsNotNull(db.Games.Single().Moves[0].SelectedSAN);
                Assert.AreEqual("e4", db.Games.Single().Moves[0].SelectedSAN!);
                Assert.AreEqual(GameState.InProgress, db.Games.Single().State);
            }
        }

        [TestMethod]
        public async Task GameRolloverNoMoves_resultsIn_Abandon()
        {
            var engine = await StartEngineAsync();
            var game = await StartGameWithDbAsync();
            await ExpireCurrentMoveShortlyAsync(game);

            WaitAndAssert(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().GamePosts.Any(p => p.Type == PostType.Engine_GameAbandoned);
            });

            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Engine_GameAbandoned),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(GameState.Abandoned, db.Games.Single().State);
                Assert.IsNotNull(db.Games.Single().Finished);
                Assert.AreEqual(1, db.Games.Single().GamePosts.Count(p => p.Type == PostType.Engine_GameAbandoned));
            }
        }

        [TestMethod]
        public async Task StatusCommand_resultsIn_StatusReply()
        {
            var engine = await StartEngineAsync();

            var command = await SendToEngineAsync($"status", authorised: true);

            EngineSocialMock.Verify(ns => ns.PostAsync(
            It.Is<Post>(p =>
                p.Succeeded == true &&
                p.Type == PostType.CommandResponse &&
                p.NetworkReplyToId == command.SourcePostId),
            null),
            Times.Once);
        }

        [TestMethod]
        public async Task AbandonCommand_abandons_GameByShortcode()
        {
            var engine = await StartEngineAsync();
            var game = await StartGameWithDbAsync();

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(GameState.InProgress, db.Games.Single().State);
            }

            var command = await SendToEngineAsync($"abandon {game.Shortcode}", authorised: true);

            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.CommandResponse &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);

            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Engine_GameAbandoned),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(GameState.Abandoned, db.Games.Single().State);
                Assert.IsNotNull(db.Games.Single().Finished);
                Assert.AreEqual(1, db.Games.Single().GamePosts.Count(p => p.Type == PostType.Engine_GameAbandoned));
            }
        }

        [TestMethod]
        public async Task AdvanceCommand_advancesAndAbandons_GameByShortcode()
        {
            var engine = await StartEngineAsync();
            var game = await StartGameWithDbAsync();

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(GameState.InProgress, db.Games.Single().State);
            }

            var command = await SendToEngineAsync($"advance {game.Shortcode}", authorised: true);

            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.CommandResponse &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);

            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Engine_GameAbandoned),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(GameState.Abandoned, db.Games.Single().State);
                Assert.IsNotNull(db.Games.Single().Finished);
                Assert.AreEqual(1, db.Games.Single().GamePosts.Count(p => p.Type == PostType.Engine_GameAbandoned));
            }
        }

        [TestMethod]
        public async Task AdvanceCommand_requires_GameShortcode()
        {
            var engine = await StartEngineAsync();
            var game = await StartGameWithDbAsync();

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(GameState.InProgress, db.Games.Single().State);
            }

            var command = await SendToEngineAsync($"advance", authorised: true);

            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.CommandRejection &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);
        }

        [TestMethod]
        public async Task AdvanceCommand_requires_ValidGameShortcode()
        {
            var engine = await StartEngineAsync();
            var game = await StartGameWithDbAsync();

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(GameState.InProgress, db.Games.Single().State);
            }

            var command = await SendToEngineAsync($"advance kryten", authorised: true);

            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.CommandRejection &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);
        }

        [TestMethod]
        public async Task StatusCommand_forGameShortcode_ShowsInfoAboutGame()
        {
            var engine = await StartEngineAsync();
            var game = await StartGameWithDbAsync();

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(GameState.InProgress, db.Games.Single().State);
            }

            var command = await SendToEngineAsync($"status {game.Shortcode}", authorised: true);

            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.CommandResponse &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);
        }

        [TestMethod]
        public async Task StatusCommand_forUnknownParameter_IsRejected()
        {
            var engine = await StartEngineAsync();
            var game = await StartGameWithDbAsync();

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(GameState.InProgress, db.Games.Single().State);
            }

            var command = await SendToEngineAsync($"status ramasses-niblet-the-third", authorised: true);

            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.CommandRejection &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);
        }

    }
}
