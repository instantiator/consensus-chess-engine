using System;
namespace ConsensusChessShared.DTO
{
	public class Media : AbstractDTO
	{
		public byte[] Data { get; set; }
		public string Alt { get; set; }
	}
}

