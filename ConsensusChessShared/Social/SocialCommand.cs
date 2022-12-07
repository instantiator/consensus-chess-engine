using System;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Social
{
	public class SocialCommand
	{
        public SocialCommand(
			Network receivingNetwork, SocialUsername username,
			string postId, string? notificationId,
			DateTime sourceCreated,
			string text,
			bool isForThisNode,
			bool isAuthorised,
			bool isRetrospective,
			bool isProcessed,
			string deliveryMedium,
			string deliveryType,
            string? inReplyTo = null)
		{
			ReceivingNetwork = receivingNetwork;
			SourceCreated = sourceCreated;
			SourceUsername = username;
			SourcePostId = postId;
			SourceNotificationId = notificationId;
			RawText = text;
			IsForThisNode = isForThisNode;
			IsAuthorised = isAuthorised;
			IsRetrospective = isRetrospective;
			IsProcessed = isProcessed;
			DeliveryMedium = deliveryMedium;
			DeliveryType = deliveryType;
			InReplyToId = inReplyTo;
		}

		public DateTime SourceCreated { get; set; }
		public Network ReceivingNetwork { get; set; }
		public SocialUsername SourceUsername { get; set; }
		public string SourcePostId { get; set; }
		public string? SourceNotificationId { get; set; }

		public string RawText { get; set; }

		public string? InReplyToId { get; set; }
		public bool IsForThisNode { get; set; }
		public bool IsAuthorised { get; set; }
		public bool IsRetrospective { get; set; }
		public bool IsProcessed { get; set; }

		public string DeliveryMedium { get; set; }
		public string DeliveryType { get; set; }
    }
}
