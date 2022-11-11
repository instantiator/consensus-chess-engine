using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConsensusChessShared.DTO
{
    public class Commitment : IDTO
    {
		public Commitment()
		{
            Created = DateTime.Now.ToUniversalTime();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime Created { get; set; }

        public string GameShortcode { get; set; }
	    public Side GameSide { get; set; }
	}
}

