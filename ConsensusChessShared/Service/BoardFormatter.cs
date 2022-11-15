using System;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Service
{
	public class BoardFormatter
	{

		public static string VisualiseEmoji(Board board)
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

