using System;
namespace ConsensusChessShared.DTO
{
	public class Post : AbstractDTO
	{
		public PostType Type { get; set; }
		public Network Network { get; set; }
		public string Message { get; set; }
		public IEnumerable<Tuple<byte[], string>> MediaPng { get; set; }
	}
}

