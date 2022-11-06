using System;
namespace ConsensusChessShared.DTO
{
	public class Participant : AbstractDTO
	{
		public Network Network { get; set; }
		public string NetworkUserId { get; set; }
		public Dictionary<Game, Side> CommittedSide { get; set; }
	}
}

