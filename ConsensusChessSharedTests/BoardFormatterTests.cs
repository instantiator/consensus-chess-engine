using System;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessSharedTests.Data;
using static ConsensusChessShared.Content.BoardFormatter;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class BoardFormatterTests
	{
		[TestMethod]
		public void FenToPieces_forFAN_LooksRight()
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
        public void FenToPieces_forFEN_LooksRight()
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
        public void FenToPieces_forWords_LooksRight()
        {
            var expected = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}",
                "Row 8: black rook, black knight, black bishop, black queen, black king, black bishop, black knight, black rook, end of row.",
                "Row 7: 8 black pawns, end of row.",
                "Row 6: 8 spaces, end of row.",
                "Row 5: 8 spaces, end of row.",
                "Row 4: 8 spaces, end of row.",
                "Row 3: 8 spaces, end of row.",
                "Row 2: 8 white pawns, end of row.",
                "Row 1: white rook, white knight, white bishop, white queen, white king, white bishop, white knight, white rook, end of row.");

            var board = new Board(); // default starting position
            var output = BoardFormatter.FenToPieces(board, BoardFormat.Words_en);
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void DescribeBoard_mentions_Checkmate()
        {
            var board = Board.FromFEN(SampleDataGenerator.FEN_FoolsMate);
            var output = BoardFormatter.DescribeBoard(board, true, BoardFormat.Words_en, null);
            Assert.IsTrue(output.Contains("The black king is checkmated."));
        }
    }
}

