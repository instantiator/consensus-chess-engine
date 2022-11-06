using System;
namespace ConsensusChessShared.DTO
{
	public class Network : AbstractDTO
	{
		public NetworkType Type { get; set; }
		public string NetworkServer { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string AppToken { get; set; }
        public string Name { get; set; }

        public string Descriptor => $"{Type}:{NetworkServer}:{Name}";

        public static Network FromEnvironment(NetworkType type, System.Collections.IDictionary env)
        {
            switch (type)
            {
                case NetworkType.Mastodon:
                    var server = (string)env["MASTODON_SERVER"];
                    var appKey = (string)env["MASTODON_APP_KEY"];
                    var appSecret = (string)env["MASTODON_APP_SECRET"];
                    var appAccessToken = (string)env["MASTODON_ACCESS_TOKEN"];
                    var name = (string)env["MASTODON_APP_NAME"];
                    return new Network()
                    {
                        Name = name,
                        Type = type,
                        NetworkServer = server,
                        AppKey = appKey,
                        AppSecret = appSecret,
                        AppToken = appAccessToken
                    };

                default:
                    throw new ArgumentException($"NetworkType {type} not supported.");
            }
        }
    }
}

