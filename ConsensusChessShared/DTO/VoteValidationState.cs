using System;
namespace ConsensusChessShared.DTO
{
	public enum VoteValidationState
	{
		Valid,
		NoGame,
		InvalidSAN,
		NotPermitted,
		RuleViolation
	}
}

