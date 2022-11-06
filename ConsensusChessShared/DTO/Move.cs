using System;
namespace ConsensusChessShared.DTO
{
	public class Move : AbstractDTO
	{
		public Board From { get; set; }
		public Board To { get; set; }
		public Vote SelectedVote { get; set; }
		public IEnumerable<Vote> Votes { get; set; }
	}
}

