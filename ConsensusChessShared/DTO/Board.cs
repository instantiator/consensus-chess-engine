using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsensusChessShared.DTO
{
	public class Board : IDTO
	{
		// FEN represents white as upper case
		public static readonly string INITIAL_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		/// <summary>
		/// Generates a default board - with all pieces and game state in starting positions.
		/// </summary>
		public Board()
		{
			Created = DateTime.Now.ToUniversalTime();
			FEN = INITIAL_FEN;
			BoardPosts = new List<Post>();
		}

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

        public string FEN { get; set; }
        public virtual List<Post> BoardPosts { get; set; }

		public string PiecesFEN => FEN.Split(" ")[0];
        public Side ActiveSide => FEN.Split(" ")[1] == "w" ? Side.White : Side.Black;
		public string CastlingFEN => FEN.Split(" ")[2];
		public string EnPassantSq => FEN.Split(" ")[3];
		public int HalfMoveClock => int.Parse(FEN.Split(" ")[4]); // increments after each side's move, 0-indexed
		public int FullMoveNumber => int.Parse(FEN.Split(" ")[5]); // increments after black's turn, 1-indexed

		public static Board FromFEN(string fen)
		{
			return new Board() { FEN = fen };
		}
	}
}

