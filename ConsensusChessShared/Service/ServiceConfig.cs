using System;
using System.Collections;
using Mastonet;

namespace ConsensusChessShared.Service
{
	public class ServiceConfig
	{
        public ServiceConfig(string tag, string admin, Visibility publicVisibility, IEnumerable<string> ignorables)
        {
            GameTag = tag;
            AdminContact = admin;
            MastodonPublicPostVisibility = publicVisibility;
            Ignorables = ignorables;
        }

        public string GameTag { get; set; }
        public string AdminContact { get; set; }
        public Visibility MastodonPublicPostVisibility { get; set; }
        public IEnumerable<string> Ignorables { get; set; }

        public static ServiceConfig FromEnv(IDictionary env)
        {
            var environment = env is Dictionary<string, string>
                ? (Dictionary<string, string>)env
                : env.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);

            var tag = environment["POST_GAME_TAG"];
            var admin = environment["POST_ADMIN_CONTACT"];
            var publicVisibility = Enum.Parse<Visibility>(environment["POST_PUBLIC_VISIBILITY"]);
            var ignorables = environment["COMMAND_IGNORE_KEYWORDS"].Split(',').Select(i => i.Trim());
            return new ServiceConfig(tag, admin, publicVisibility, ignorables);
        }
    }
}

