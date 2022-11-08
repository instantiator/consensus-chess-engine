using System;
namespace ConsensusChessShared.DTO
{
	public class Node : AbstractDTO
	{
		public string Name { get; set; }
		public Network Network { get; set; }
		public long LastCommandId { get; set; }
	}
}

