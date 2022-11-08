using System;
using System.Collections;

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
        public string AuthorisedAccounts { get; set; }

        public string Descriptor => $"{Type}:{NetworkServer}:{Name}";
        public IEnumerable<string> AuthorisedAccountsList => AuthorisedAccounts.Split(',').Select(a => a.TrimStart('@'));

        public static Network FromEnvironment(NetworkType type, System.Collections.IDictionary env)
        {
            var environment = env.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);

            switch (type)
            {
                case NetworkType.Mastodon:
                    var name = environment["NETWORK_APP_NAME"];
                    var server = environment["NETWORK_SERVER"];
                    var appKey = environment["NETWORK_APP_KEY"];
                    var appSecret = environment["NETWORK_APP_SECRET"];
                    var appAccessToken = environment["NETWORK_ACCESS_TOKEN"];
                    var authorisedAccounts = environment["AUTHORISED_ACCOUNTS"];

                    return new Network()
                    {
                        Name = name,
                        Type = type,
                        NetworkServer = server,
                        AppKey = appKey,
                        AppSecret = appSecret,
                        AppToken = appAccessToken,
                        AuthorisedAccounts = authorisedAccounts
                    };

                default:
                    throw new ArgumentException($"NetworkType {type} not supported.");
            }
        }
    }
}

