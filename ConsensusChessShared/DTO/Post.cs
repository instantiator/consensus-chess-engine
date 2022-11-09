using System;
namespace ConsensusChessShared.DTO
{
	public class Post : AbstractDTO
	{
		public PostType Type { get; set; }
		public string NetworkName { get; set; }
		public string Message { get; set; }
		public IEnumerable<Media> MediaPng { get; set; }
	}
}

