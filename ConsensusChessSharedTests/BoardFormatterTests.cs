using System;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using static ConsensusChessShared.Content.BoardFormatter;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class BoardFormatterTests
	{
		[TestMethod]
		public void CanFormat_FAN()
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
            var output = BoardFormatter.FenToPieces(board, BoardFormat.StandardFAN);
			Assert.AreEqual(expected, output);
		}

        [TestMethod]
        public void CanFormat_FEN()
        {
            var expected = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}",
                "rnbqkbnr/",
                "pppppppp/",
                "8/",
                "8/",
                "8/",
                "8/",
                "PPPPPPPP/",
                "RNBQKBNR");

            var board = new Board(); // default starting position
            var output = BoardFormatter.FenToPieces(board, BoardFormat.StandardFEN);
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void CanFormat_Words_en()
        {
            var expected = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}",
                "black rook, black knight, black bishop, black queen, black king, black bishop, black knight, black rook, end of row.",
                "8 black pawns, end of row.",
                "8 spaces, end of row.",
                "8 spaces, end of row.",
                "8 spaces, end of row.",
                "8 spaces, end of row.",
                "8 white pawns, end of row.",
                "white rook, white knight, white bishop, white queen, white king, white bishop, white knight, white rook, end.");

            var board = new Board(); // default starting position
            var output = BoardFormatter.FenToPieces(board, BoardFormat.Words_en);
            Assert.AreEqual(expected, output);
        }
    }
}

