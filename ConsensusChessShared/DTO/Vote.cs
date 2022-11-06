using System;
namespace ConsensusChessShared.DTO
{
	public class Vote : AbstractDTO
	{
		public string MoveText { get; set; }
		public Participant Participant { get; set; }
		public VoteValidation Validation { get; set; }
	}
}

