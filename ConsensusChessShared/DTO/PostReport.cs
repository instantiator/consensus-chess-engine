using System;
using System.ComponentModel.DataAnnotations.Schema;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.DTO
{
	public class PostReport : AbstractDTO
	{
		public bool Succeeded { get; set; }
		public Post Post { get; set; }
		public string? ErrorMessage { get; set; }
		public string? ExceptionType { get; set; }

		[NotMapped]
		public Exception? Exception { get; set; }

        public static PostReport Success(Post post)
		{
			return new PostReport()
			{
                Succeeded = true,
				Post = post
			};
		}

        public static PostReport From(Exception exception, Post post)
		{
			return new PostReport()
			{
                Succeeded = false,
				ErrorMessage = exception.Message,
				ExceptionType = exception.GetType().Name,
				Exception = exception,
				Post = post
			};
		}
	}
}

