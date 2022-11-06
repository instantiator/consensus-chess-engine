using System;
namespace ConensusChessShared.DTO
{
	public class Participant : AbstractDTO
	{
		public Network Network { get; set; }
		public string NetworkUserId { get; set; }
	}
}

