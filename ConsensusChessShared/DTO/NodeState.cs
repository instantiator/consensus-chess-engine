using System;
using System.Collections;
namespace ConsensusChessShared.DTO
{
	public class NodeState : AbstractDTO
	{
		public string NodeName { get; set; }
		public long LastNotificationId { get; set; }
        public long LastReplyId { get; set; }

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

