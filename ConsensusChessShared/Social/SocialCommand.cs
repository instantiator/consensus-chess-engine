using System;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Social
{
	public class SocialCommand
	{
		public Network Network { get; set; }
        public string NetworkUserId { get; set; }
        public string RawText { get; set; }
		public long? SourceId { get; set; }
		public long? InReplyToId { get; set; }
		public string SourceAccount { get; set; }
		public bool IsForThisNode { get; set; }
		public bool IsAuthorised { get; set; }
		public bool IsRetrospective { get; set; }
		public bool IsProcessed { get; set; }

		public string? DeliveryMedium { get; set; }
		public string? DeliveryType { get; set; }
	}
}
