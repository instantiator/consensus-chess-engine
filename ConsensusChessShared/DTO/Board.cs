using System;
namespace ConsensusChessShared.DTO
{
	public class Board : AbstractDTO
	{
		public string FEN { get; set; }
		public IEnumerable<Post> Posts { get; set; }
	}
}

