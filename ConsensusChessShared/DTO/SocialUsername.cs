using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConsensusChessShared.Constants;

namespace ConsensusChessShared.DTO
{
	public class SocialUsername : IDTO
	{
		public SocialUsername()
		{
			Created = DateTime.Now.ToUniversalTime();
		}

		private SocialUsername(string username, string network, string display, NetworkType type) : this()
		{
			Username = username;
			Server = network;
			DisplayName = display;
			NetworkType = type;
		}

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

		public NetworkType NetworkType { get; set; }
		public string Username { get; set; }
		public string Server { get; set; }
		public string DisplayName { get; set; }

		public string Full => $"{Username}@{Server}";
        public string AtFull => $"@{Username}@{Server}";
		public string AtUsername => $"@{Username}";

        public static SocialUsername From(string input, string display, Network receivingNetwork)
		{
			string username;
			string server;

			// split on @
			var parts = input.Split('@').Where(s => !string.IsNullOrWhiteSpace(s));

			switch (parts.Count())
			{
				case 1:
					// if there's 1 item, it's the username, set network from the receiving network
					username = parts.ElementAt(0);
					server = receivingNetwork.NetworkServer;
					break;
				case 2:
					// if there are 2, it's the username and the network
					username = parts.ElementAt(0);
					server = parts.ElementAt(1);
					break;

				default:
					// if there's 0 or more than 2, bork out - it's not a username
					throw new ArgumentException($"Username provided had {parts.Count()} parts: {input}");
			}

			return new SocialUsername(username.ToLower(), server.ToLower(), display, receivingNetwork.Type);
		}

        public override bool Equals(object? obj)
        {
            if (obj is string)
            {
                var str = (string)obj;
				return str == Username || str == Full || str == AtFull;
            }
			if (obj is SocialUsername)
			{
				var username = (SocialUsername)obj;
				return username.Full == Full;
			}
			return false;
        }

        public override int GetHashCode()
        {
            return Full.GetHashCode();
        }
    }
}

