using System;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Social
{
	public class SocialCommand
	{
		public Network Network { get; set; }
        public string NetworkUserId { get; set; }
        public string RawText { get; set; }
	}
}

