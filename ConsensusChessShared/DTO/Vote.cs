using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsensusChessShared.DTO
{
	public class Vote : IDTO
    {
		public Vote()
		{
            Created = DateTime.Now.ToUniversalTime();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

		public long NetworkMovePostId { get; set; }
		public string MoveText { get; set; }
		public virtual Participant Participant { get; set; }

		public VoteValidationState ValidationState { get; set; }
		public virtual Post? ValidationPost { get; set; }
	}
}

