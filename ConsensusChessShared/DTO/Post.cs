using System;
namespace ConsensusChessShared.DTO
{
	public class Post : AbstractDTO
	{
		public Post()
		{
			MediaPng = new List<Media>();
		}

		public PostType Type { get; set; }
		public string NetworkName { get; set; }
		public string Message { get; set; }
		public List<Media> MediaPng { get; set; }
		public long? ReplyTo { get; set; }
	}
}

