using System;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Social
{
	public class SocialCommand
	{
        public SocialCommand(
			Network receivingNetwork, SocialUsername username, long postId,
			string text,
			bool isForThisNode,
			bool isAuthorised,
			bool isRetrospective,
			bool isProcessed,
			string deliveryMedium,
			string deliveryType,
            long? inReplyTo = null)
		{
			ReceivingNetwork = receivingNetwork;
			SourceUsername = username;
			SourcePostId = postId;
			RawText = text;
			IsForThisNode = isForThisNode;
			IsAuthorised = isAuthorised;
			IsRetrospective = isRetrospective;
			IsProcessed = isProcessed;
			DeliveryMedium = deliveryMedium;
			DeliveryType = deliveryType;
			InReplyToId = inReplyTo;
		}

		public Network ReceivingNetwork { get; set; }
		public SocialUsername SourceUsername { get; set; }
		public long SourcePostId { get; set; }

		public string RawText { get; set; }

		public long? InReplyToId { get; set; }
		public bool IsForThisNode { get; set; }
		public bool IsAuthorised { get; set; }
		public bool IsRetrospective { get; set; }
		public bool IsProcessed { get; set; }

		public string DeliveryMedium { get; set; }
		public string DeliveryType { get; set; }
    }
}
