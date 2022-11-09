using System;
namespace ConsensusChessShared.DTO
{
	public class VoteValidation : AbstractDTO
	{
		public bool ValidationState { get; set; }
		public string Note { get; set; }
		public PostReport VoteValidationPost { get; set; }
	}
}

