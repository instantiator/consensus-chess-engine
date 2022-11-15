using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsensusChessShared.DTO
{
	public class Post : IDTO
    {
		public Post()
		{
			MediaPng = new List<Media>();
            Created = DateTime.Now.ToUniversalTime();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

        public PostType Type { get; set; }
		public string NetworkServer { get; set; }
		public string AppName { get; set; }
		public string NodeShortcode { get; set; }
		public string Message { get; set; }
		public virtual List<Media> MediaPng { get; set; }

        public long? NetworkPostId { get; set; }
        public long? NetworkReplyToId { get; set; }

        public DateTime? Attempted { get; set; }
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ExceptionType { get; set; }

        [NotMapped]
        public Exception? Exception { get; set; }

        public void Succeed()
        {
            Attempted = DateTime.Now.ToUniversalTime();
            Succeeded = true;
        }

        public void Fail(string? message = null, Exception? e = null)
        {
            Attempted = DateTime.Now.ToUniversalTime();
            ErrorMessage = message ?? e?.Message ?? "Unspecified error";
            Exception = e;
            ExceptionType = e?.GetType().FullName;
            Succeeded = false;
        }
    }
}

