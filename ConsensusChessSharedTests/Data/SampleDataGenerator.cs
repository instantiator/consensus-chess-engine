using System;
using System.Collections;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;

namespace ConsensusChessSharedTests.Data
{
	public class SampleDataGenerator
	{
        public static string FEN_PreFoolsMate = "rnbqkbnr/3ppppp/ppp5/8/2B5/4PQ2/PPPP1PPP/RNB1K1NR w KQkq - 0 1";
        public static string FEN_FoolsMate    = "rnbqkbnr/3ppQpp/ppp5/8/2B5/4P3/PPPP1PPP/RNB1K1NR b KQkq - 0 1";
        public static string SAN_FoolsMate    = "Qf7";

        public static long RollingPostId = 1;

		public static string[] AuthorisedAccounts =>
            new[]
            {
                "@instantiator@mastodon.social", // the author
                "@icgames@botsin.space"          // the engine
            };

        public static string[] Skips =>
            new[]
            {
                "@icgames", "@icgames@botsin.space"
            };

        public static Network FakeNetwork =>
            new Network()
            {
                AppName = "fake network",
                NetworkServer = "fake.mastodon.server"
            };

        public static SocialUsername FakeSelf =>
            SocialUsername.From("UnitTest", "Unit test", FakeNetwork);

        public static NodeState NodeState =>
            new NodeState()
            {
                Name = "simple test node",
                Shortcode = "node-0-test",
                LastNotificationId = 0,
                Network = FakeNetwork
            };

        public static SocialCommand SimpleCommand(string message, bool isAuthorised = false, bool isRetrospective = false, string? sender = null)
        {
            return new SocialCommand(
                receivingNetwork: FakeNetwork,
                username: SocialUsername.From(sender ?? "instantiator@mastodon.social", "lewis", FakeNetwork),
                postId: RollingPostId++,
                text: message,
                isForThisNode: true,
                isAuthorised: isAuthorised,
                isRetrospective: isRetrospective,
                isProcessed: false,
                deliveryMedium: "UnitTests",
                deliveryType: "SimpleCommand");
        }

        public static IDictionary SimpleNetworkEnv => new Hashtable()
        {
            {"NETWORK_TYPE","Mastodon"},
            {"NETWORK_SERVER","some.kind.of.mastodon"},
            {"NETWORK_APP_NAME","app-name"},
            {"NETWORK_APP_KEY","app-key"},
            {"NETWORK_APP_SECRET","app-secret"},
            {"NETWORK_ACCESS_TOKEN","access-token"},
            {"NETWORK_AUTHORISED_ACCOUNTS","instantiator@mastodon.social,icgames@botsin.space"},
            {"NETWORK_DRY_RUNS","true"},
        };
    }
}

