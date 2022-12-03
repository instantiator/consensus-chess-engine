using System;
using System.Collections;

namespace ConsensusChessShared.Service
{
	public class ServiceConfig
	{
        public ServiceConfig(string tag, string admin)
        {
            GameTag = tag;
            AdminContact = admin;
        }

        public string GameTag { get; set; }
        public string AdminContact { get; set; }

        public static ServiceConfig FromEnv(IDictionary env)
        {
            var environment = env is Dictionary<string, string>
                ? (Dictionary<string, string>)env
                : env.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);

            var tag = environment["POST_GAME_TAG"];
            var admin = environment["POST_ADMIN_CONTACT"];
            return new ServiceConfig(tag, admin);
        }
    }
}

