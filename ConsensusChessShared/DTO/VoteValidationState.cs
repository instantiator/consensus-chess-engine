using System;
namespace ConsensusChessShared.DTO
{
	public enum VoteValidationState
	{
		Valid,
		NoGame,
		InvalidSAN,
		OffSide,
		Superceded
	}
}

