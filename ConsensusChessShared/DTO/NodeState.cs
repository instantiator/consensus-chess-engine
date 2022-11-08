using System;
using System.Collections;
namespace ConsensusChessShared.DTO
{
	public class NodeState : AbstractDTO
	{
		public string NodeName { get; set; }
		public long LastMentionId { get; set; }
        public long LastReplyId { get; set; }

		public static NodeState Create(string name)
		{
			return new NodeState()
			{
				NodeName = name,
				LastMentionId = 0,
				LastReplyId = 0
			};
        }
    }
}

