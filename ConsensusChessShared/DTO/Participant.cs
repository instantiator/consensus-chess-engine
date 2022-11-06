using System;
namespace ConsensusChessShared.DTO
{
	public class Participant : AbstractDTO
	{
		public Network Network { get; set; }
		public string NetworkUserId { get; set; }
		public IEnumerable<Commitment> Commitments { get; set; }
	}
}

