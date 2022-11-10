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
        public string AppName { get; set; }
        public string AuthorisedAccounts { get; set; }

        public string Descriptor => $"{Type}:{NetworkServer}:{AppName}";
        public IEnumerable<string> AuthorisedAccountsList => AuthorisedAccounts.Split(',').Select(a => a.TrimStart('@'));

        public static Network FromEnvironment(IDictionary env)
        {
            var environment = env.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);

            var networkType = Enum.Parse<NetworkType>(environment["NETWORK_TYPE"]);
            var appName = environment["NETWORK_APP_NAME"];
            var server = environment["NETWORK_SERVER"];
            var appKey = environment["NETWORK_APP_KEY"];
            var appSecret = environment["NETWORK_APP_SECRET"];
            var appAccessToken = environment["NETWORK_ACCESS_TOKEN"];
            var authorisedAccounts = environment["NETWORK_AUTHORISED_ACCOUNTS"];

            return new Network()
            {
                Type = networkType,
                NetworkServer = server,
                AppName = appName,
                AppKey = appKey,
                AppSecret = appSecret,
                AppToken = appAccessToken,
                AuthorisedAccounts = authorisedAccounts
            };
        }
    }
}

