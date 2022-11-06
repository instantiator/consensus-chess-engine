using System;
namespace ConsensusChessShared.DTO
{
	public class Board : AbstractDTO
	{
		public char[][] Data { get; set; }
		public IEnumerable<Post> Posts { get; set; }
	}
}

