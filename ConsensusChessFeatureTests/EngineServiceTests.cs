using System;
using ConsensusChessFeatureTests.Data;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;
using Moq;

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
                    p.Message == "This instruction can't be processed: UnrecognisedCommand" &&
                    p.NetworkReplyToId == command.SourceId),
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

            var command = FeatureDataGenerator.GenerateCommand($"new {NodeId.Shortcode}", EngineNetwork);

            await receivers[EngineId.Shortcode].Invoke(command);

            // akcknowledgement
            EngineSocialMock.Verify(ns => ns.PostAsync(
            It.Is<Post>(p =>
                p.Succeeded == true &&
                p.Message == $"New MoveLock game for: {NodeId.Shortcode}" &&
                p.NetworkReplyToId == command.SourceId),
            null),
            Times.Once);

            // announcement
            EngineSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Message == "New MoveLock game...\nWhite: \nBlack: \nMove duration: 2.00:00:00"),
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

            var command = FeatureDataGenerator.GenerateCommand($"new {NodeId.Shortcode}", EngineNetwork, authorised: false);

            await receivers[EngineId.Shortcode].Invoke(command);

            EngineSocialMock.Verify(ns => ns.PostAsync(
            It.Is<Post>(p =>
                p.Succeeded == true &&
                p.Message == "This instruction can't be processed: NotAuthorised" &&
                p.NetworkReplyToId == command.SourceId),
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

            var command = FeatureDataGenerator.GenerateCommand($"new beans-on-toast", EngineNetwork);

            await receivers[EngineId.Shortcode].Invoke(command);

            await Task.Delay(1000);

            EngineSocialMock.Verify(ns => ns.PostAsync(
            It.Is<Post>(p =>
                p.Succeeded == true &&
                p.Message == "Unrecognised shortcodes: beans-on-toast" &&
                p.NetworkReplyToId == command.SourceId),
            null),
            Times.Once);

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(0, db.Games.Count());
            }
        }

    }
}
