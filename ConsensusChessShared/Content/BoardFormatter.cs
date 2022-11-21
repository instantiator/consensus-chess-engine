using System;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Content
{
	public class BoardFormatter
	{

		public static string FenToPieces(Board board)
		{
            return board.PiecesFEN
                .Replace("/", "/\n")
                .Replace('R', '♖')
                .Replace('N', '♘')
                .Replace('B', '♗')
                .Replace('Q', '♕')
                .Replace('K', '♔')
                .Replace('P', '♙')
                .Replace('r', '♜')
                .Replace('n', '♞')
                .Replace('b', '♝')
                .Replace('q', '♛')
                .Replace('k', '♚')
                .Replace('p', '♟');
        }


    }
}

