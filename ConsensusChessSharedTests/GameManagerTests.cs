using System;
using Chess;
using ConsensusChessEngine.Service;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using ConsensusChessSharedTests.Data;
using Microsoft.EntityFrameworkCore;
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
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test Game", "a test game", new[] { "mastodon.something.social" }, new[] { "node-0-test" });

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
        public void CurrentBoardWithoutPost_finds_firstBoard()
        {
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test Game", "a test game", new[] { "network.server" }, new[] { "node-0-test" });
            var board = gm.CurrentBoardWithoutPost(game, "node-0-test", PostType.Node_BoardUpdate);
            Assert.IsNotNull(board);
            Assert.AreSame(game.CurrentBoard, board);
        }

        [TestMethod]
        public void ValidateSAN_validates_GoodVoteSAN()
        {
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test Game", "a test game", new[] { "mastodon.something.social" }, new[] { "node-0-test" });
            var vote = new Vote() { MoveText = "e4" };
            var move = gm.NormaliseAndValidateMoveTextToSAN(game.CurrentBoard, vote);

            var board = gm.ApplyValidatedMoveText(game.CurrentBoard, move);
            Assert.IsNotNull(board);
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", board.FEN);
        }

        [TestMethod]
        public void ValidateSAN_invalidates_IllegalVoteSAN()
        {
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test Game", "a test game", new[] { "mastodon.something.social" }, new[] { "node-0-test" });
            var vote = new Vote() { MoveText = "e7" };
            var e = Assert.ThrowsException<VoteRejectionException>(() =>
            {
                gm.NormaliseAndValidateMoveTextToSAN(game.CurrentBoard, vote);
            });
            Assert.AreEqual(typeof(ChessSanNotFoundException), e.InnerException!.GetType());
        }

        [TestMethod]
        public void ValidateSAN_invalidates_GarbageVote()
        {
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test Game", "a test game", new[] { "mastodon.something.social" }, new[] { "node-0-test" });
            var vote = new Vote() { MoveText = "horsey to king 4" };
            var e = Assert.ThrowsException<VoteRejectionException>(() =>
            {
                gm.NormaliseAndValidateMoveTextToSAN(game.CurrentBoard, vote);
            });
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
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test Game", "a test game", null, new[] { "node-0-test" });
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

            var game = gm.CreateSimpleMoveLockGame("test-game", "Test Game", "a test game", null, new[] { "node-0-test" });
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
            var game = gm.CreateSimpleMoveLockGame("test-game", "Test Game", "a test game", null, new[] { "node-0-test" });

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

        [TestMethod]
        public void NormaliseAndValidateMoveTextToSAN_tests()
        {
            var board = new Board();

            Assert.AreEqual("e4", gm.NormaliseAndValidateMoveTextToSAN(board, new Vote() { MoveText = "e2 - e4" }));
            Assert.AreEqual("e4", gm.NormaliseAndValidateMoveTextToSAN(board, new Vote() { MoveText = "e4" }));
        }

        [TestMethod]
        public void FindUnpostedActiveBoards_GameWithNoPosts_FindsTheGame()
        {
            var shortcode = SampleDataGenerator.NodeState.Shortcode;
            var game = SampleDataGenerator.SimpleMoveLockGame();

            var dbGames = MockDbHelper.GetQueryableMockDbSet<Game>("games");
            dbGames.Add(game);

            var results = gm.FindUnpostedActiveGameBoards(dbGames, shortcode, PostType.Node_BoardUpdate);

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(game, results.First().Key);
            Assert.IsNotNull(results.First().Value);
            Assert.AreEqual(0, results.First().Value!.BoardPosts.Count());
        }

        [TestMethod]
        public void FindUnpostedActiveBoards_GameWithSomeOtherPosts_FindsTheGame()
        {
            var shortcode = SampleDataGenerator.NodeState.Shortcode;
            var game = SampleDataGenerator.SimpleMoveLockGame();
            var post = new Post()
            {
                Type = PostType.Node_BoardUpdate,
                NodeShortcode = "not-the-node-shortcode"
            };
            game.CurrentBoard.BoardPosts.Add(post);

            var dbGames = MockDbHelper.GetQueryableMockDbSet<Game>("games");
            dbGames.Add(game);

            var results = gm.FindUnpostedActiveGameBoards(dbGames, shortcode, PostType.Node_BoardUpdate);

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(game, results.First().Key);
            Assert.IsNotNull(results.First().Value);
            Assert.AreEqual(1, results.First().Value!.BoardPosts.Count());
        }

        [TestMethod]
        public void FindUnpostedActiveBoards_GameAlreadyPosted_DoesNotFindTheGame()
        {
            var shortcode = SampleDataGenerator.NodeState.Shortcode;
            var game = SampleDataGenerator.SimpleMoveLockGame();
            var post = new Post()
            {
                Type = PostType.Node_BoardUpdate,
                NodeShortcode = shortcode
            };
            game.CurrentBoard.BoardPosts.Add(post);

            var dbGames = MockDbHelper.GetQueryableMockDbSet<Game>("games");
            dbGames.Add(game);

            var results = gm.FindUnpostedActiveGameBoards(dbGames, shortcode, PostType.Node_BoardUpdate);

            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        public void FindUnpostedEndedGames_GameInProgress_DoesNotFindTheGame()
        {
            var shortcode = SampleDataGenerator.NodeState.Shortcode;
            var game = SampleDataGenerator.SimpleMoveLockGame();

            var dbGames = MockDbHelper.GetQueryableMockDbSet<Game>("games");
            dbGames.Add(game);

            var results = gm.FindUnpostedEndedGames(dbGames, shortcode);

            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        public void FindUnpostedEndedGames_GameAbandoned_FindsTheGame()
        {
            var shortcode = SampleDataGenerator.NodeState.Shortcode;
            var game = SampleDataGenerator.SimpleMoveLockGame();
            game.State = GameState.Abandoned;

            var dbGames = MockDbHelper.GetQueryableMockDbSet<Game>("games");
            dbGames.Add(game);

            var results = gm.FindUnpostedEndedGames(dbGames, shortcode);

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(game, results.Single());
        }

        [TestMethod]
        public void FindUnpostedEndedGames_GameEnded_FindsTheGame()
        {
            var shortcode = SampleDataGenerator.NodeState.Shortcode;
            var game = SampleDataGenerator.SimpleMoveLockGame();

            var dbGames = MockDbHelper.GetQueryableMockDbSet<Game>("games");
            dbGames.Add(game);

            game.State = GameState.Stalemate;
            Assert.AreEqual(1, gm.FindUnpostedEndedGames(dbGames, shortcode).Count());
            Assert.AreEqual(game, gm.FindUnpostedEndedGames(dbGames, shortcode).Single());

            game.State = GameState.BlackKingCheckmated;
            Assert.AreEqual(1, gm.FindUnpostedEndedGames(dbGames, shortcode).Count());
            Assert.AreEqual(game, gm.FindUnpostedEndedGames(dbGames, shortcode).Single());

            game.State = GameState.WhiteKingCheckmated;
            Assert.AreEqual(1, gm.FindUnpostedEndedGames(dbGames, shortcode).Count());
            Assert.AreEqual(game, gm.FindUnpostedEndedGames(dbGames, shortcode).Single());

            game.State = GameState.InProgress;
            Assert.AreEqual(0, gm.FindUnpostedEndedGames(dbGames, shortcode).Count());
        }

        [TestMethod]
        public void CountVotes_withValidVotes_CorrectlyCounts()
        {
            var game = SampleDataGenerator.SimpleMoveLockGame();
            for (var i = 0; i < 5; i++)
            {
                game.CurrentMove.Votes.Add(new Vote(
                    SampleDataGenerator.RollingPostId++,
                    "e2 - e4",
                    SampleDataGenerator.SampleParticipant($"participant-{i}"),
                    "e4",
                    VoteValidationState.Valid));
            }
            for (var i = 5; i < 8; i++)
            {
                game.CurrentMove.Votes.Add(new Vote(
                    SampleDataGenerator.RollingPostId++,
                    "e2 - e3",
                    SampleDataGenerator.SampleParticipant($"participant-{i}"),
                    "e3",
                    VoteValidationState.Valid));
            }
            var votes = gm.CountVotes(game.CurrentMove);

            Assert.AreEqual(5, votes["e4"]);
            Assert.AreEqual(3, votes["e3"]);
        }
    }
}
