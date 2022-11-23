using System;
using System.Collections;

namespace ConsensusChessShared.Service
{
	public class ServiceIdentity
	{
		public ServiceIdentity(string name, string shortcode)
		{
			Name = name;
			Shortcode = shortcode;
		}

		public string Name { get; set; }
		public string Shortcode { get; set; }

		public static ServiceIdentity FromEnv(IDictionary env)
		{
            var environment = env is Dictionary<string, string>
                ? (Dictionary<string, string>)env
                : env.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);

            var name = environment["NODE_NAME"];
            var shortcode = environment["NODE_SHORTCODE"];
			return new ServiceIdentity(name, shortcode);
        }
    }
}

