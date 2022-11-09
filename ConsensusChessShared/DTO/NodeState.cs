using System;
using System.Collections;
using System.Net.Mail;
using ConsensusChessShared.Social;
using Microsoft.EntityFrameworkCore;

namespace ConsensusChessShared.DTO
{
    [Index(nameof(Shortcode), IsUnique = true)]
    public class NodeState : AbstractDTO
	{
		public NodeState()
		{
			StatePosts = new List<PostReport>();
		}

		public string Name { get; set; }

		public string Shortcode { get; set; }
		public long LastNotificationId { get; set; }
        public long LastReplyId { get; set; }
		public List<PostReport> StatePosts { get; set; }

		public static NodeState Create(string name, string shortcode)
		{
			return new NodeState()
			{
				Name = name,
				Shortcode = shortcode,
				LastNotificationId = 0,
				LastReplyId = 0
			};
        }
    }
}

