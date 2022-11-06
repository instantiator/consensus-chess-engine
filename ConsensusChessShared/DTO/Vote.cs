using System;
namespace ConsensusChessShared.DTO
{
	public class Vote : AbstractDTO
	{
		public Move Move { get; set; }
		public Participant Participant { get; set; }
		public MoveValidation Validation { get; set; }
	}
}

