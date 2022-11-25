using System;
using Chess;
using ConsensusChessSharedTests.Data;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class ChessLibTests
	{
		ChessBoard NewBoard() => new ChessBoard();

		[TestInitialize]
		public void Init()
		{
		}

		[TestMethod]
        public void SanWithBoardSucceeds()
        {
            Assert.IsTrue(NewBoard().IsValidMove("c4"));
            Assert.IsTrue(NewBoard().Move("c4"));
        }

        [TestMethod]
		public void SanWithoutBoardFails()
		{

			Assert.ThrowsException<ChessArgumentException>(() =>
			{
                var move = new Move("c4");
            });
		}

		[TestMethod]
		public void MoveByPositionSucceeds()
		{
			var move = new Move("c2","c4");
			Assert.IsNotNull(move);

			var board = NewBoard();
			Assert.IsTrue(board.IsValidMove(move));
            Assert.IsTrue(board.Move(move));

			board = NewBoard();
            string? san;
			var ok = board.TryParseToSan(move, out san);
			Assert.IsTrue(ok);
			Assert.IsNotNull(move.San);
			Assert.AreEqual("c4", move.San);
			Assert.IsNull(san); // TODO: in future versions of the library, hopefully san will not be null
		}

		[TestMethod]
		public void CheckmateDetected()
		{
			var board = ChessBoard.LoadFromFen(SampleDataGenerator.FEN_PreFoolsMate);

			var boardStr = board.ToAscii();
			var moves = string.Join("\n", board.Moves().Select(m => m.San));

			Assert.IsFalse(board.IsEndGame);
			Assert.IsTrue(board.IsValidMove(SampleDataGenerator.SAN_FoolsMate));

			// enact the move
			board.Move(SampleDataGenerator.SAN_FoolsMate);
			Assert.AreEqual(SampleDataGenerator.FEN_FoolsMate, board.ToFen());
			Assert.AreEqual(PieceColor.Black, board.Turn); // turn flips regardless of checkmate

			// end game
			Assert.IsTrue(board.IsEndGame);
			Assert.IsTrue(board.BlackKingChecked);
			Assert.IsFalse(board.WhiteKingChecked);
		}
	}
}

