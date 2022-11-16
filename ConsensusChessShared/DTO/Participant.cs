using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConsensusChessShared.Social;

namespace ConsensusChessShared.DTO
{
	public class Participant : IDTO
    {
		public Participant()
		{
            Created = DateTime.Now.ToUniversalTime();
            Commitments = new List<Commitment>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

		public string NetworkUserAccount { get; set; }
        public string NetworkServer { get; set; }
		public virtual List<Commitment> Commitments { get; set; }

        public static Participant From(SocialCommand command)
        {
            return new Participant()
            {
                NetworkUserAccount = command.SourceAccount,
                NetworkServer = command.SourceAccount.Split('@').Last()
            };
        }
	}
}
