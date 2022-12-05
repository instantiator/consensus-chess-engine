using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConsensusChessShared.Constants;

namespace ConsensusChessShared.DTO
{
	public class Network : IDTO
    {
        public Network()
        {
            Created = DateTime.Now.ToUniversalTime();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

        public NetworkType Type { get; set; }
		public string NetworkServer { get; set; }
        public string AppToken { get; set; }
        public string AppName { get; set; }
        public string ExpectedAccountName { get; set; }
        public string AuthorisedAccounts { get; set; }
        public bool DryRuns { get; set; }

        public string Descriptor => $"{Type}:{NetworkServer}:{AppName}";
        public IEnumerable<string> AuthorisedAccountsList => AuthorisedAccounts.Split(',').Select(a => a.TrimStart('@'));

        public static Network FromEnv(IDictionary env)
        {
            var environment = env is Dictionary<string, string>
                ? (Dictionary<string,string>)env
                : env.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);

            var networkType = Enum.Parse<NetworkType>(environment["NETWORK_TYPE"]);
            var appName = environment["NETWORK_APP_NAME_REMINDER"];
            var server = environment["NETWORK_SERVER"];
            var expectedAccountName = environment["NETWORK_ACCOUNT_NAME"];
            var appAccessToken = environment["NETWORK_ACCESS_TOKEN"];
            var authorisedAccounts = environment["NETWORK_AUTHORISED_ACCOUNTS"];
            var dryRuns = bool.Parse(environment["NETWORK_DRY_RUNS"]);

            return new Network()
            {
                Type = networkType,
                NetworkServer = server,
                AppName = appName,
                AppToken = appAccessToken,
                AuthorisedAccounts = authorisedAccounts,
                DryRuns = dryRuns,
                ExpectedAccountName = expectedAccountName
            };
        }
    }
}

