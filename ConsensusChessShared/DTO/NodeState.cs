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
			LastNotificationId = null;
			LastCommandStatusId = null;
		}

		public NodeState(string name, string shortcode, Network network) : this()
		{
			this.Name = name;
			this.Shortcode = shortcode;
			this.Network = network;
		}

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
		public DateTime Created { get; set; }

		public string Name { get; set; }

		public string Shortcode { get; set; }
		public string? LastNotificationId { get; set; }
		public string? LastCommandStatusId { get; set; }
        public virtual List<Post> StatePosts { get; set; }
		public virtual Network Network { get; set; }
    }
}

