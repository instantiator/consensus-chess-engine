using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsensusChessShared.DTO
{
	public class Media : IDTO
    {
		public Media()
		{
            Created = DateTime.Now.ToUniversalTime();
        }

        public Media(string filename, byte[] data, string alt) : this()
        {
            Filename = filename;
            Data = data;
            Alt = alt;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

        public byte[] Data { get; set; }
		public string Alt { get; set; }
        public string Filename { get; set; }

        public string? SocialId { get; set; }
        public string? PreviewUrl { get; set; }
    }
}

