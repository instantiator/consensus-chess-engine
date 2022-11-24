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
                    p.Message == "This instruction can't be processed: UnrecognisedCommand" &&
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

            var command = FeatureDataGenerator.GenerateCommand($"new {NodeId.Shortcode}", EngineNetwork);
            await receivers[EngineId.Shortcode].Invoke(command);

            // akcknowledgement
            EngineSocialMock.Verify(ns => ns.PostAsync(
            It.Is<Post>(p =>
                p.Succeeded == true &&
                p.Message == $"New MoveLock game for: {NodeId.Shortcode}" &&
                p.NetworkReplyToId == command.SourcePostId),
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

                Assert.AreEqual(1, game.GamePosts.Count(p => p.Type == PostType.Engine_GameAnnouncement));
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

            var command = FeatureDataGenerator.GenerateCommand($"new beans-on-toast", EngineNetwork);

            await receivers[EngineId.Shortcode].Invoke(command);

            // await Task.Delay(1000);

            EngineSocialMock.Verify(ns => ns.PostAsync(
            It.Is<Post>(p =>
                p.Succeeded == true &&
                p.Message == "Unrecognised shortcodes: beans-on-toast" &&
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

            // create game
            var newGame = Game.NewGame("game-shortcode", "description",
                    new[] { NodeNetwork.NetworkServer },
                    new[] { NodeNetwork.NetworkServer },
                    new[] { NodeId.Shortcode },
                    new[] { NodeId.Shortcode },
                    SideRules.MoveLock);

            using (var db = Dbo.GetDb())
            {
                db.Games.Add(newGame);
                await db.SaveChangesAsync();
            }

            // get the board post
            long? boardPost1id;
            SpinWait.SpinUntil(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
            });
            using (var db = Dbo.GetDb())
            {
                var game = db.Games.Single();
                boardPost1id = game.CurrentBoard.BoardPosts.Single().NetworkPostId!;
                Assert.IsNotNull(boardPost1id);
            }

            // add some votes
            for (var i = 0; i < 5; i++)
            {
                var voteCmd = FeatureDataGenerator.GenerateCommand($"move e4", EngineNetwork, false, from: $"voter-{i}", inReplyTo: boardPost1id);
                await receivers[NodeId.Shortcode].Invoke(voteCmd);

                using (var db = Dbo.GetDb())
                {
                    var game = db.Games.Single();
                    Assert.AreEqual(i + 1, game.CurrentMove.Votes.Count());
                    Assert.IsNotNull(game.CurrentMove.Votes.Last().ValidationPost);
                    Assert.AreEqual(VoteValidationState.Valid, game.CurrentMove.Votes.Last().ValidationState);
                    Assert.AreEqual("e4", game.CurrentMove.Votes.Last().MoveText);
                }
            }
            for (var i = 5; i < 8; i++)
            {
                var voteCmd = FeatureDataGenerator.GenerateCommand($"move e3", EngineNetwork, false, from: $"voter-{i}", inReplyTo: boardPost1id);
                await receivers[NodeId.Shortcode].Invoke(voteCmd);

                using (var db = Dbo.GetDb())
                {
                    var game = db.Games.Single();
                    Assert.AreEqual(i + 1, game.CurrentMove.Votes.Count());
                    Assert.IsNotNull(game.CurrentMove.Votes.Last().ValidationPost);
                    Assert.AreEqual(VoteValidationState.Valid, game.CurrentMove.Votes.Last().ValidationState);
                    Assert.AreEqual("e3", game.CurrentMove.Votes.Last().MoveText);
                }
            }

            // modify game to expire shortly
            using (var db = Dbo.GetDb())
            {
                var game = db.Games.Single();
                game.CurrentMove.Deadline = DateTime.Now.Add(TimeSpan.FromSeconds(1)).ToUniversalTime();
                await db.SaveChangesAsync();
            }

            // wait until there's a new move
            SpinWait.SpinUntil(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().Moves.Count() == 2;
            });
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
            }
        }

        [TestMethod]
        public async Task GameRolloverNoMoves_resultsIn_Abandon()
        {
            var engine = await StartEngineAsync();
            var node = await StartNodeAsync();

            // create game
            var newGame = Game.NewGame("game-shortcode", "description",
                    new[] { NodeNetwork.NetworkServer },
                    new[] { NodeNetwork.NetworkServer },
                    new[] { NodeId.Shortcode },
                    new[] { NodeId.Shortcode },
                    SideRules.MoveLock);

            using (var db = Dbo.GetDb())
            {
                db.Games.Add(newGame);
                await db.SaveChangesAsync();
            }

            // modify game to expire shortly
            using (var db = Dbo.GetDb())
            {
                var game = db.Games.Single();
                game.CurrentMove.Deadline = DateTime.Now.Add(TimeSpan.FromSeconds(1)).ToUniversalTime();
                await db.SaveChangesAsync();
            }

            // wait until there's a new move
            SpinWait.SpinUntil(() =>
            {
                using (var db = Dbo.GetDb())
                    //return db.Games.Single().State == GameState.Abandoned;
                    return db.Games.Single().GamePosts.Any(p => p.Type == PostType.Engine_GameAbandoned);
            });
            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(GameState.Abandoned, db.Games.Single().State);
                Assert.IsNotNull(db.Games.Single().Finished);
                Assert.AreEqual(1, db.Games.Single().GamePosts.Count(p => p.Type == PostType.Engine_GameAbandoned));
            }

        }

    }
}
