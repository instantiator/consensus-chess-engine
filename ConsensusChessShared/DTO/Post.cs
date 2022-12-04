using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using Mastonet;

namespace ConsensusChessShared.DTO
{
	public class Post : IDTO
    {
		public Post()
		{
			Media = new List<Media>();
            Created = DateTime.Now.ToUniversalTime();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

        // filled during preparation
        public PostType Type { get; set; }
		public string? Message { get; set; }
		public virtual List<Media> Media { get; set; }
        public Visibility? OverrideMastodonVisibility { get; set; }

        // filled during send
		public string? NetworkServer { get; set; }
		public string? AppName { get; set; }
		public string? NodeShortcode { get; set; }
        public long? NetworkPostId { get; set; }
        public long? NetworkReplyToId { get; set; }
        public DateTime? Attempted { get; set; }
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ExceptionType { get; set; }

        [NotMapped]
        public Exception? Exception { get; set; }

        public void Succeed(string shortcode, string appname, string networkserver, long? networkPostId)
        {
            NodeShortcode = shortcode;
            AppName = appname;
            NetworkServer = networkserver;

            Attempted = DateTime.Now.ToUniversalTime();
            NetworkPostId = networkPostId;
            Succeeded = true;
        }

        public void Fail(string shortcode, string appname, string networkserver, string? message = null, Exception? e = null)
        {
            NodeShortcode = shortcode;
            AppName = appname;
            NetworkServer = networkserver;

            Attempted = DateTime.Now.ToUniversalTime();
            ErrorMessage = message ?? e?.Message ?? "Unspecified error";
            Exception = e;
            ExceptionType = e?.GetType().FullName;
            Succeeded = false;
        }
    }
}

