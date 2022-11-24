using System;
namespace ConsensusChessShared.Constants
{
	public enum VoteValidationState
	{
		Unchecked,
		Valid,
		NoGame,
		InvalidMoveText,
		OffSide,
		Superceded
	}
}

