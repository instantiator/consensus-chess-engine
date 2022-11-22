using System;
namespace ConsensusChessShared.Constants
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

