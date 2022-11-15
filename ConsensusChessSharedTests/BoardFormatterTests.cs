using System;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class BoardFormatterTests
	{
		[TestMethod]
		public void CanFormatVisualBoard()
		{
			var expected = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}",
				"♜♞♝♛♚♝♞♜/",
				"♟♟♟♟♟♟♟♟/",
				"8/",
				"8/",
				"8/",
				"8/",
				"♙♙♙♙♙♙♙♙/",
				"♖♘♗♕♔♗♘♖");

			var board = new Board(); // default starting position
            var output = BoardFormatter.VisualiseEmoji(board);
			Assert.AreEqual(output, expected);
		}
	}
}

