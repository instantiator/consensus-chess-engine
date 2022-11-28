using System;
using System.Text;
using Chess;
using ConsensusChessShared.DTO;
using Mastonet.Entities;

namespace ConsensusChessShared.Content
{
	public class BoardFormatter
	{
        public enum BoardFormat
        {
            StandardFEN,
            StandardFAN,
            Words_en,
            ASCII
        }

        public static string FenToPieces(Board board, BoardFormat style)
		{
            switch (style)
            {
                case BoardFormat.ASCII:
                    return ChessBoard.LoadFromFen(board.FEN).ToAscii();

                case BoardFormat.Words_en:
                    return $"{MapPieces(board.PiecesFEN, style)}end.";

                default:
                    return MapPieces(board.PiecesFEN, style);
            }
        }

        public static string MapPieces(string fen, BoardFormat style)
        {
            StringBuilder sb = new StringBuilder();
            int position = 0;
            while (position < fen.Count())
            {
                var mapping = Mappings[style]
                    .FirstOrDefault(mapping => fen.Substring(position).StartsWith(mapping.Key));

                var found = !default(KeyValuePair<string, string>).Equals(mapping);
                var mappedValue = found ? mapping.Value : fen[position].ToString();

                sb.Append(mappedValue);
                position += found ? mapping.Key.Length : 1;
            }
            return sb.ToString();
        }

        public static Dictionary<BoardFormat, Dictionary<string, string>> Mappings =
            new Dictionary<BoardFormat, Dictionary<string, string>>()
            {
                {
                    // a minor modification to standard fen - pop a new line after each row
                    BoardFormat.StandardFEN,
                    new Dictionary<string,string>()
                    {
                        { "/", "/\n" },
                    }
                },
                {
                    // replace standard fen letters with unicode symbols
                    BoardFormat.StandardFAN,
                    new Dictionary<string,string>()
                    {
                        { "/","/\n" },
                        { "R","♖" },
                        { "N","♘" },
                        { "B","♗" },
                        { "Q","♕" },
                        { "K","♔" },
                        { "P","♙" },
                        { "r","♜" },
                        { "n","♞" },
                        { "b","♝" },
                        { "q","♛" },
                        { "k","♚" },
                        { "p","♟" }
                    }
                },
                {
                    // describe the board (as best as possible) row by row
                    BoardFormat.Words_en,
                    new Dictionary<string,string>()
                    {
                        { "PPPPPPPP","8 white pawns, " },
                        { "PPPPPPP","7 white pawns, " },
                        { "PPPPPP","6 white pawns, " },
                        { "PPPPP","5 white pawns, " },
                        { "PPPP","4 white pawns, " },
                        { "PPP","3 white pawns, " },
                        { "PP","2 white pawns, " },
                        { "P","white pawn, " },

                        { "pppppppp","8 black pawns, " },
                        { "ppppppp","7 black pawns, " },
                        { "pppppp","6 black pawns, " },
                        { "ppppp","5 black pawns, " },
                        { "pppp","4 black pawns, " },
                        { "ppp","3 black pawns, " },
                        { "pp","2 black pawns, " },
                        { "p","black pawn, " },

                        { "/","end of row.\n" },
                        { "R","white rook, " },
                        { "N","white knight, " },
                        { "B","white bishop, " },
                        { "Q","white queen, " },
                        { "K","white king, " },
                        { "r","black rook, " },
                        { "n","black knight, " },
                        { "b","black bishop, " },
                        { "q","black queen, " },
                        { "k","black king, " },

                        { "1","space, " },
                        { "2","2 spaces, " },
                        { "3","3 spaces, " },
                        { "4","4 spaces, " },
                        { "5","5 spaces, " },
                        { "6","6 spaces, " },
                        { "7","7 spaces, " },
                        { "8","8 spaces, " }
                    }
                },

            };
    }
}

