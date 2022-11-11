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
        public long Id { get; set; }
        public DateTime Created { get; set; }

		public string MoveText { get; set; }
		public virtual Participant Participant { get; set; }
		public virtual VoteValidation Validation { get; set; }
	}
}

