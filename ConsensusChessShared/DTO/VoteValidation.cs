using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsensusChessShared.DTO
{
	public class VoteValidation : IDTO
    {
		public VoteValidation()
		{
            Created = DateTime.Now.ToUniversalTime();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

	    public bool ValidationState { get; set; }
		public string Note { get; set; }
		public virtual Post VoteValidationPost { get; set; }
	}
}

