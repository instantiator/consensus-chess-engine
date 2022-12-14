using System;
using ConsensusChessFeatureTests.Service;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConsensusChessFeatureTests.Data
{
	public class FeatureDataGenerator
	{
        private static long rollingPostId { get; set; } = 1;
        private static long rollingNotificationId { get; set; } = 1;
        public static string NextPostId => rollingPostId++.ToString();
        public static string NextNotificationId => rollingNotificationId++.ToString();

        public static string FEN_PreSimpleCheckmate = "rnbqkbnr/3ppppp/ppp5/8/2B5/4PQ2/PPPP1PPP/RNB1K1NR w KQkq - 0 1";
        public static string FEN_SimpleCheckmate = "rnbqkbnr/3ppQpp/ppp5/8/2B5/4P3/PPPP1PPP/RNB1K1NR b KQkq - 0 1";
        public static string SAN_SimpleCheckmate = "Qf7";

        public static Dictionary<string, string> NodeEnv =>
            new Dictionary<string, string>()
            {
                { "NODE_NAME", "a fake node" },
                { "NODE_SHORTCODE", "node-0-fake" },
                { "NETWORK_TYPE", "Mastodon" },
                { "NETWORK_SERVER", "fake.mastodon.server" },
                { "NETWORK_ACCOUNT_NAME", "icgames" },
                { "NETWORK_APP_NAME_REMINDER", "node-app" },
                { "NETWORK_ACCESS_TOKEN", "access-token" },
                { "NETWORK_AUTHORISED_ACCOUNTS", "engine" },
                { "NETWORK_DRY_RUNS", "false" },
                { "POST_GAME_TAG", "#ConsensusChessFeatureTests" },
                { "POST_ADDITIONAL_TAGS", "#FeatureTest #PleaseIgnore" },
                { "POST_ADMIN_CONTACT", "@instantiator@mastodon.social" },
                { "POST_PUBLIC_VISIBILITY", "Unlisted" },
                { "COMMAND_IGNORE_KEYWORDS", "#hush,#ignore" },
                { "STREAM_ENABLED", "true" }
            };

        public static Dictionary<string, string> EngineEnv =>
            new Dictionary<string, string>()
            {
                { "NODE_NAME", "a fake engine" },
                { "NODE_SHORTCODE", "engine-fake" },
                { "NETWORK_TYPE", "Mastodon" },
                { "NETWORK_SERVER", "fake.mastodon.server" },
                { "NETWORK_APP_NAME_REMINDER", "engine-app" },
                { "NETWORK_ACCOUNT_NAME", "icgames_engine" },
                { "NETWORK_ACCESS_TOKEN", "access-token" },
                { "NETWORK_AUTHORISED_ACCOUNTS", "admin@chesstodon.social" },
                { "NETWORK_DRY_RUNS", "false" },
                { "POST_GAME_TAG", "#ConsensusChessFeatureTests" },
                { "POST_ADDITIONAL_TAGS", "#FeatureTest #PleaseIgnore" },
                { "POST_ADMIN_CONTACT", "@instantiator@mastodon.social" },
                { "POST_PUBLIC_VISIBILITY", "Unlisted" },
                { "COMMAND_IGNORE_KEYWORDS", "#hush,#ignore" },
                { "STREAM_ENABLED", "true" }
            };

        public static Post SimulatePost(Post post, string shortcode, Network network)
        {
            post.Attempted = DateTime.Now.ToUniversalTime();
            post.AppName = network.AppName;
            post.NetworkServer = network.NetworkServer;
            post.NodeShortcode = shortcode;
            post.Succeeded = true;
            post.NetworkPostId = NextPostId;
            return post;
        }

        public static Post GeneratePost(string shortcode, Network network, string message, PostType? type)
        {
            return new Post()
            {
                AppName = network.AppName,
                Attempted = DateTime.Now,
                Created = DateTime.Now,
                Message = message,
                NetworkServer = network.NetworkServer,
                NodeShortcode = shortcode,
                Succeeded = true,
                Type = type ?? PostType.Unspecified,
                NetworkPostId = NextPostId,
            };
        }

        public static SocialCommand GenerateCommand(string message, Network network, bool authorised = true, string? from = null, string? inReplyTo = null)
        {
            var fromUser = from ?? "instantiator";

            return new SocialCommand(
                receivingNetwork: network,
                username: SocialUsername.From(fromUser, $"Display name for {fromUser}", network),
                sourceCreated: DateTime.Now,
                postId: NextPostId,
                notificationId: NextNotificationId,
                text: message,
                isForThisNode: true,
                isAuthorised: authorised,
                isRetrospective: false,
                isProcessed: false,
                deliveryMedium: "FeatureTest",
                deliveryType: "GenerateCommand",
                inReplyTo: inReplyTo);
        }
    }
}

