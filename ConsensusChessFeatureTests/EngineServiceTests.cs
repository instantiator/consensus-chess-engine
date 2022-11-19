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

            var command = new SocialCommand()
            {
                DeliveryMedium = "mock",
                DeliveryType = "notification",
                IsAuthorised = true,
                IsRetrospective = false,
                NetworkUserId = "instantiator",
                IsForThisNode = true,
                RawText = "hello",
                ReceivingNetwork = NodeNetwork,
                SourceAccount = "instantiator",
                SourceId = FeatureDataGenerator.RollingPostId++
            };

            await receivers[EngineId.Shortcode].Invoke(command);

            EngineSocialMock.Verify(ns => ns.ReplyAsync(
                command,
                "UnrecognisedCommand",
                PostType.CommandResponse,
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

            var command = new SocialCommand()
            {
                DeliveryMedium = "mock",
                DeliveryType = "notification",
                IsAuthorised = true,
                IsRetrospective = false,
                NetworkUserId = "instantiator",
                IsForThisNode = true,
                RawText = $"new {NodeId.Shortcode}",
                ReceivingNetwork = NodeNetwork,
                SourceAccount = "instantiator",
                SourceId = FeatureDataGenerator.RollingPostId++
            };

            await receivers[EngineId.Shortcode].Invoke(command);

            EngineSocialMock.Verify(ns => ns.ReplyAsync(
                command,
                $"New MoveLock game for: {NodeId.Shortcode}",
                PostType.CommandResponse,
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
    }
}

