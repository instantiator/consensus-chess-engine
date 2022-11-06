using System;
namespace ConsensusChessShared.DTO
{
	public class MoveValidation : AbstractDTO
	{
		public bool ValidationState { get; set; }
		public string Note { get; set; }
		public Post Post { get; set; }
	}
}

