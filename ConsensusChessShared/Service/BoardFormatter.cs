using System;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Service
{
	public class BoardFormatter
	{

		public static string VisualiseEmoji(Board board)
		{
            return board.PiecesFEN
                .Replace("1", "◽️")
                .Replace("2", "◽️◽️")
                .Replace("3", "◽️◽️◽️")
                .Replace("4", "◽️◽️◽️◽️")
                .Replace("5", "◽️◽️◽️◽️◽️")
                .Replace("6", "◽️◽️◽️◽️◽️◽️")
                .Replace("7", "◽️◽️◽️◽️◽️◽️◽️")
                .Replace("8", "◽️◽️◽️◽️◽️◽️◽️◽️")
                .Replace('/', '\n')
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

