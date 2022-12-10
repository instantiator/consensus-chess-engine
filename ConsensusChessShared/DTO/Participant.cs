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

        public Participant(SocialUsername username) : this()
        {
            Username = username;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

        public virtual SocialUsername Username { get; set; }
		public virtual List<Commitment> Commitments { get; set; }

        public static Participant From(SocialCommand command)
        {
            return new Participant(command.SourceUsername);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) { return false; }
            if (this == obj) { return true; }
            if (!(obj is Participant)) { return false; }
            var participant = obj as Participant;
            return Username.Equals(participant!.Username);
        }
    }
}
