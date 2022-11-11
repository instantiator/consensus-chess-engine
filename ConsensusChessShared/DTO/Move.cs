﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public long Id { get; set; }
        public DateTime Created { get; set; }

        public virtual Board From { get; set; }
		public virtual Board? To { get; set; }
		public virtual Vote? SelectedVote { get; set; }
		public Side SideToPlay { get; set; }
		public virtual List<Vote> Votes { get; set; }
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

