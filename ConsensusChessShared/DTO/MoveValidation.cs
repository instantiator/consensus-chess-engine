using System;
namespace ConensusChessShared.DTO
{
	public class MoveValidation : AbstractDTO
	{
		public bool IsValid { get; set; }
		public Post Post { get; set; }
	}
}

