using System;
using System.Collections;
using ConsensusChessShared.Social;

namespace ConsensusChessShared.DTO
{
	public class NodeState : AbstractDTO
	{
		public NodeState()
		{
			StatePosts = new List<PostReport>();
		}

		public string NodeName { get; set; }
		public long LastNotificationId { get; set; }
        public long LastReplyId { get; set; }
		public List<PostReport> StatePosts { get; set; }

		public static NodeState Create(string name)
		{
			return new NodeState()
			{
				NodeName = name,
				LastNotificationId = 0,
				LastReplyId = 0
			};
        }
    }
}

