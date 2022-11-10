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
				"♜♞♝♛♚♝♞♜",
				"♟♟♟♟♟♟♟♟",
				"◽️◽️◽️◽️◽️◽️◽️◽️",
				"◽️◽️◽️◽️◽️◽️◽️◽️",
				"◽️◽️◽️◽️◽️◽️◽️◽️",
				"◽️◽️◽️◽️◽️◽️◽️◽️",
				"♙♙♙♙♙♙♙♙",
				"♖♘♗♕♔♗♘♖");

            var output = BoardFormatter.PiecesFENtoVisualEmoji(Board.INITIAL_PIECES_FEN);
			Assert.AreEqual(output, expected);
		}
	}
}

