using System;
using System.Drawing;
using Chess;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Content
{
	public class BoardRenderer
	{
        private Board board;
        private ChessBoard chessboard;

		public BoardRenderer(Board board)
		{
            this.board = board;
            this.chessboard = ChessBoard.LoadFromFen(board.FEN);
		}

        //private Bitmap DrawFilledRectangle(int x, int y)
        //{
        //    Bitmap bmp = new Bitmap(x, y);
        //    using (Graphics graph = Graphics.FromImage(bmp))
        //    {
        //        Rectangle ImageSize = new Rectangle(0, 0, x, y);
        //        graph.FillRectangle(Brushes.White, ImageSize);
        //    }
        //    return bmp;
        //}
    }
}

