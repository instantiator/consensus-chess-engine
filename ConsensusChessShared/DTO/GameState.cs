using System;
namespace ConsensusChessShared.DTO
{
	public enum GameState
	{
		InProgress,
		WhiteCheckmate,
		BlackCheckmate,
		Stalemate,
		Abandoned
	}
}

