using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConsensusChessShared.Constants;

namespace ConsensusChessShared.DTO
{
	public class Move : IDTO
    {
		public Move()
		{
			Votes = new List<Vote>();
            Created = DateTime.Now.ToUniversalTime();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

        public virtual Board From { get; set; }
		public virtual Board? To { get; set; }
		public virtual string? SelectedSAN { get; set; }
		public virtual List<Vote> Votes { get; set; }
		public DateTime Deadline { get; set; }

		public bool Expired => DateTime.Now.ToUniversalTime() > Deadline;
		public Side SideToPlay => From.ActiveSide;

		[NotMapped]
		public TimeSpan? TimeRemaining => Deadline > DateTime.Now.ToUniversalTime()
			? Deadline.Subtract(DateTime.Now.ToUniversalTime())
			: null;

		public static Move CreateStartingMove(TimeSpan duration)
		{
			return new Move()
			{
				From = new Board(),
				Deadline = DateTime.Now.Add(duration).ToUniversalTime(),
			};
		}
	}
}

