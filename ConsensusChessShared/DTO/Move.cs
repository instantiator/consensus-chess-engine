using System;
namespace ConsensusChessShared.DTO
{
	public class Move : AbstractDTO
	{
		public Move() : base()
		{
			Votes = new List<Vote>();
		}

		public Board From { get; set; }
		public Board? To { get; set; }
		public Vote? SelectedVote { get; set; }
		public Side SideToPlay { get; set; }
		public List<Vote> Votes { get; set; }
		public DateTime Deadline { get; set; }

		public static Move CreateStartingMove(TimeSpan duration)
		{
			return new Move()
			{
				From = Board.CreateStartingBoard(),
				Deadline = DateTime.Now.Add(duration).ToUniversalTime(),
				SideToPlay = Side.White
			};
		}
	}
}

