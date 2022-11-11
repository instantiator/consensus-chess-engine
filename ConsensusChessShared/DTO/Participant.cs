using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsensusChessShared.DTO
{
	public class Participant : IDTO
    {
		public Participant()
		{
            Created = DateTime.Now.ToUniversalTime();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

		public string NetworkUserId { get; set; }
        public string NetworkServer { get; set; }
		public virtual IEnumerable<Commitment> Commitments { get; set; }
	}
}

