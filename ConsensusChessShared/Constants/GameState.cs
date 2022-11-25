using System;
namespace ConsensusChessShared.Constants
{
	public enum GameState
	{
		InProgress,
		WhiteKingCheckmated,
		BlackKingCheckmated,
		Stalemate,
		Abandoned
	}
}

