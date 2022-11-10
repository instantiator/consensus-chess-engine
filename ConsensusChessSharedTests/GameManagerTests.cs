using System;
using ConsensusChessEngine.Service;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class GameManagerTests
	{

        private Mock<ILogger> mockLogger;
        private GameManager gm;

        [TestInitialize]
        public void Init()
        {
            mockLogger = new Mock<ILogger>();
            gm = new GameManager(mockLogger.Object);
        }

        [TestMethod]
        public void CreateSimpleMoveLockGame_creates_Game()
        {
            var game = gm.CreateSimpleMoveLockGame(new[] { "node-0-test" });

            Assert.IsNotNull(game);
            Assert.AreEqual(SideRules.MoveLock, game.SideRules);
            Assert.IsTrue(game.ScheduledStart <= DateTime.Now);
            Assert.AreEqual(Game.DEFAULT_MOVE_DURATION, game.MoveDuration);
            Assert.AreEqual(1, game.WhiteNetworks.Count());
            Assert.AreEqual(1, game.BlackNetworks.Count());
            Assert.AreEqual(1, game.Moves.Count());

            var move = game.Moves.First();
            Assert.IsTrue(DateTime.Now <= move.Deadline);
            Assert.AreEqual(Side.White, move.SideToPlay);
            Assert.AreEqual(0, move.Votes.Count());
            Assert.IsNotNull(move.From);
            Assert.IsNull(move.To);

            var board = move.From;
            Assert.AreEqual(0, board.BoardPosts.Count());
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", board.Pieces_FEN);
            Assert.AreEqual(Side.White, board.ActiveSide);
            Assert.AreEqual("KQkq", board.CastlingAvailability_FEN);
            Assert.AreEqual("-", board.EnPassantTargetSquare_FEN);
            Assert.AreEqual(0, board.HalfMoveClock);
            Assert.AreEqual(1, board.FullMoveNumber);
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", board.FEN);
        }
    }
}

