using System;
namespace ConsensusChessShared.DTO
{
	public class Board : AbstractDTO
	{
		// white are upper case
		public static readonly string INITIAL_PIECES_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
		public static readonly string INITIAL_CASTLING_FEN = "KQkq";

		public Board() : base()
		{
			BoardPosts = new List<PostReport>();
		}

		public string Pieces_FEN { get; set; }
		public Side ActiveSide { get; set; }
		public string CastlingAvailability_FEN { get; set; }
		public string EnPassantTargetSquare_FEN { get; set; }
		public int HalfMoveClock { get; set; }
		public int FullMoveNumber { get; set; }

		public string FEN
		{
			get
			{
				var side = ActiveSide == Side.White ? "w" : "b";
				return $"{Pieces_FEN} {side} {CastlingAvailability_FEN} {EnPassantTargetSquare_FEN} {HalfMoveClock} {FullMoveNumber}";
			}
		}

		public List<PostReport> BoardPosts { get; set; }

		public static Board CreateStartingBoard()
		{
			return new Board()
			{
				Pieces_FEN = INITIAL_PIECES_FEN,
				ActiveSide = Side.White,
				CastlingAvailability_FEN = INITIAL_CASTLING_FEN,
				EnPassantTargetSquare_FEN = "-",
				HalfMoveClock = 0,
				FullMoveNumber = 1
            };
		}
	}
}

