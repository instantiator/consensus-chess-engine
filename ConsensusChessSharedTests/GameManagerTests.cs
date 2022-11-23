using System;
using Chess;
using ConsensusChessEngine.Service;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using ConsensusChessSharedTests.Data;
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
            var game = gm.CreateSimpleMoveLockGame("test-game","Test game", new[] { "mastodon.something.social" }, new[] { "node-0-test" });

            Assert.IsNotNull(game);
            Assert.AreEqual(SideRules.MoveLock, game.SideRules);
            Assert.IsTrue(game.ScheduledStart <= DateTime.Now);
            Assert.AreEqual(Game.DEFAULT_MOVE_DURATION, game.MoveDuration);
            Assert.AreEqual(1, game.WhiteParticipantNetworkServers!.Count());
            Assert.AreEqual(1, game.BlackParticipantNetworkServers!.Count());
            Assert.AreEqual(1, game.WhitePostingNodeShortcodes.Count());
            Assert.AreEqual(1, game.BlackPostingNodeShortcodes.Count());
            Assert.AreEqual(1, game.Moves.Count());

            var move = game.Moves.First();
            Assert.IsTrue(DateTime.Now <= move.Deadline);
            Assert.AreEqual(Side.White, move.SideToPlay);
            Assert.AreEqual(0, move.Votes.Count());
            Assert.IsNotNull(move.From);
            Assert.IsNull(move.To);

            var board = move.From;
            Assert.AreEqual(0, board.BoardPosts.Count());
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", board.PiecesFEN);
            Assert.AreEqual(Side.White, board.ActiveSide);
            Assert.AreEqual("KQkq", board.CastlingFEN);
            Assert.AreEqual("-", board.EnPassantSq);
            Assert.AreEqual(0, board.HalfMoveClock);
            Assert.AreEqual(1, board.FullMoveNumber);
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", board.FEN);
        }

        [TestMethod]
        public void UnpostedBoardOrNull_finds_firstBoard()
        {
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test game", new[] { "network.server" }, new[] { "node-0-test" });
            var board = gm.UnpostedBoardOrNull(game, "node-0-test");
            Assert.IsNotNull(board);
            Assert.AreSame(game.CurrentBoard, board);
        }

        [TestMethod]
        public void ValidateSAN_validates_GoodVoteSAN()
        {
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test game", new[] { "mastodon.something.social" }, new[] { "node-0-test" });
            var vote = new Vote() { MoveText = "e4" };
            var board = gm.ValidateSAN(game.CurrentBoard, vote);
            Assert.IsNotNull(board);
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", board.FEN);
        }

        [TestMethod]
        public void ValidateSAN_invalidates_IllegalVoteSAN()
        {
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test game", new[] { "mastodon.something.social" }, new[] { "node-0-test" });
            var vote = new Vote() { MoveText = "e7" };
            var e = Assert.ThrowsException<VoteRejectionException>(() => gm.ValidateSAN(game.CurrentBoard, vote));
            Assert.AreEqual(typeof(ChessSanNotFoundException), e.InnerException!.GetType());
        }

        [TestMethod]
        public void ValidateSAN_invalidates_GarbageVote()
        {
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test game", new[] { "mastodon.something.social" }, new[] { "node-0-test" });
            var vote = new Vote() { MoveText = "horsey to king 4" };
            var e = Assert.ThrowsException<VoteRejectionException>(() => gm.ValidateSAN(game.CurrentBoard, vote));
            Assert.AreEqual(typeof(ChessArgumentException), e.InnerException!.GetType());
        }

        [TestMethod]
        public void ParticipantMayVote_withMoveLockGame_permits_NewParticipantFromAnyNetwork()
        {
            var network = new Network()
            {
                NetworkServer = "mastodon.something.social"
            };

            // no need to specify networks for move lock game
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test game", null, new[] { "node-0-test" });
            var cmd = SampleDataGenerator.SimpleCommand(message: "anything", sender: "somebody@mastodon.somewhere");
            var participant = Participant.From(cmd);
            var ok = gm.ParticipantOnSide(game, participant);
            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void ParticipantMayVote_withMoveLockGame_permits_ParticipantCommittedToCurrentSide()
        {
            var network = new Network()
            {
                NetworkServer = "mastodon.something.social"
            };

            var game = gm.CreateSimpleMoveLockGame("test-game", "Test game", null, new[] { "node-0-test" });
            var cmd = SampleDataGenerator.SimpleCommand(message: "anything", sender: "somebody@mastodon.somewhere");
            var participant = Participant.From(cmd);
            participant.Commitments.Add(new Commitment()
            {
                GameShortcode = game.Shortcode,
                GameSide = Side.White
            });

            var ok = gm.ParticipantOnSide(game, participant);
            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void ParticipantMayVote_withMoveLockGame_rejects_ParticipantAlreadyCommittedToTheOtherSide()
        {
            var network = new Network()
            {
                NetworkServer = "mastodon.somethingelse.social"
            };
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test game", null, new[] { "node-0-test" });

            var cmd = SampleDataGenerator.SimpleCommand(message: "anything", sender: "somebody@mastodon.somewhere");
            var participant = Participant.From(cmd);
            participant.Commitments.Add(new Commitment()
            {
                GameShortcode = game.Shortcode,
                GameSide = Side.Black
            });

            var ok = gm.ParticipantOnSide(game, participant);
            Assert.IsFalse(ok);
        }

    }
}

