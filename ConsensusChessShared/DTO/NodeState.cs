using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;
using ConsensusChessShared.Social;
using Microsoft.EntityFrameworkCore;

namespace ConsensusChessShared.DTO
{
    [Index(nameof(Shortcode), IsUnique = true)]
    public class NodeState : IDTO
    {
		public NodeState()
		{
			StatePosts = new List<Post>();
			Created = DateTime.Now.ToUniversalTime();
		}

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
		public DateTime Created { get; set; }

		public string Name { get; set; }

		public string Shortcode { get; set; }
		public long LastNotificationId { get; set; }
		public virtual List<Post> StatePosts { get; set; }

		public static NodeState Create(string name, string shortcode)
		{
			return new NodeState()
			{
				Name = name,
				Shortcode = shortcode,
				LastNotificationId = 0
			};
        }
    }
}

