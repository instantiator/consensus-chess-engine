using System;
namespace ConsensusChessShared.Social
{
	public class PostReport
	{
		public bool Succeeded { get; set; }
		public string? ErrorMessage { get; set; }
		public Exception? Exception { get; set; }

        public static PostReport Success()
		{
			return new PostReport()
			{
                Succeeded = true
			};
		}

        public static PostReport From(Exception exception)
		{
			return new PostReport()
			{
                Succeeded = false,
				ErrorMessage = exception.Message,
				Exception = exception
			};
		}
	}
}

