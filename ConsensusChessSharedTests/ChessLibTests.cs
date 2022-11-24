using System;
using Chess;

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

			// TODO: in future versions of the library, hopefully san will not be null
			Assert.IsNull(san);
			Assert.AreNotEqual("c4", san);
			//var san = board.ParseToSan(move);
			//Assert.AreEqual("c4", san);
		}
	}
}

