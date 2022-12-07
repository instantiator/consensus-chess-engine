using System;
using System.Collections;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;

namespace ConsensusChessSharedTests.Data
{
	public class SampleDataGenerator
	{
        public static string FEN_PreFoolsMate = "rnbqkbnr/3ppppp/ppp5/8/2B5/4PQ2/PPPP1PPP/RNB1K1NR w KQkq - 0 1";
        public static string FEN_FoolsMate    = "rnbqkbnr/3ppQpp/ppp5/8/2B5/4P3/PPPP1PPP/RNB1K1NR b KQkq - 0 1";
        public static string SAN_FoolsMate    = "Qf7";

        private static long rollingPostId = 1;
        private static long rollingNotificationId = 1;

        public static string NextPostId => rollingPostId++.ToString();
        public static string NextNotificationId => rollingNotificationId++.ToString();

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
                LastNotificationId = null,
                LastCommandStatusId = null,
                Network = FakeNetwork
            };

        public static SocialCommand SimpleCommand(string message, bool isAuthorised = false, bool isRetrospective = false, string? sender = null)
        {
            return new SocialCommand(
                receivingNetwork: FakeNetwork,
                username: SocialUsername.From(sender ?? "instantiator@mastodon.social", "lewis", FakeNetwork),
                postId: NextPostId,
                sourceCreated: DateTime.Now,
                notificationId: NextNotificationId,
                text: message,
                isForThisNode: true,
                isAuthorised: isAuthorised,
                isRetrospective: isRetrospective,
                isProcessed: false,
                deliveryMedium: "UnitTests",
                deliveryType: "SimpleCommand");
        }

        public static Game SimpleMoveLockGame()
            => new Game(
                shortcode: NodeState.Shortcode, title: "Simple MoveLock", description: "a simple movelock game",
                whiteSideNetworkServers: new[] { NodeState.Network.NetworkServer },
                blackSideNetworkServers: new[] { NodeState.Network.NetworkServer },
                whitePostingNodeShortcodes: new[] { NodeState.Shortcode },
                blackPostingNodeShortcodes: new[] { NodeState.Shortcode },
                SideRules.MoveLock);

        public static Vote SampleVote()
            => new Vote(
                SampleDataGenerator.NextPostId,
                "e2 - e4",
                SampleParticipant());

        public static Participant SampleParticipant(string? name = null)
            => new Participant(SampleUsername(name));

        public static SocialUsername SampleUsername(string? name = null)
            => SocialUsername.From(
                $"@{name ?? "instantiator"}@mastodon.social", name ?? "instantiator",
                FakeNetwork);

        public static PostBuilderFactory PostBuilderFactory
            => new PostBuilderFactory(ServiceConfig.FromEnv(SimpleConfig));

        public static IDictionary SimpleNetworkEnv => new Hashtable()
        {
            {"NETWORK_TYPE","Mastodon"},
            {"NETWORK_SERVER","some.kind.of.mastodon"},
            {"NETWORK_ACCOUNT_NAME","icgames"},
            {"NETWORK_APP_NAME_REMINDER","app-name"},
            {"NETWORK_ACCESS_TOKEN","access-token"},
            {"NETWORK_AUTHORISED_ACCOUNTS","instantiator@mastodon.social,icgames@botsin.space"},
            {"NETWORK_DRY_RUNS","true"},
        };

        public static IDictionary SimpleConfig => new Hashtable()
        {
            { "POST_ADMIN_CONTACT", "@instantiator@mastodon.social" },
            { "POST_GAME_TAG", "#ConsensusChessUnitTests" },
            { "POST_PUBLIC_VISIBILITY", "Unlisted" },
            { "COMMAND_IGNORE_KEYWORDS", "#hush,#ignore" },
            { "STREAM_ENABLED", "true" }
        };
    }
}

