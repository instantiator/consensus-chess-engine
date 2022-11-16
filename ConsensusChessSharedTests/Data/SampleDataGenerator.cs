﻿using System;
using System.Collections;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;

namespace ConsensusChessSharedTests.Data
{
	public class SampleDataGenerator
	{
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

        public static NodeState NodeState =>
            new NodeState()
            {
                Name = "simple test node",
                Shortcode = "node-0-test"
            };

        public static SocialCommand SimpleCommand(string message, bool isAuthorised = false, bool isRetrospective = false, string? sender = null)
        {
            return new SocialCommand()
            {
                IsAuthorised = isAuthorised,
                ReceivingNetwork = FakeNetwork,
                IsRetrospective = isRetrospective,
                NetworkUserId = sender ?? "@instantiator@mastodon.social",
                SourceId = 1981,
                RawText = message
            };
        }

        public static IDictionary SimpleNetworkEnv => new Hashtable()
        {
            {"NETWORK_TYPE","Mastodon"},
            {"NETWORK_SERVER","some.kind.of.mastodon"},
            {"NETWORK_APP_NAME","app-name"},
            {"NETWORK_APP_KEY","app-key"},
            {"NETWORK_APP_SECRET","app-secret"},
            {"NETWORK_ACCESS_TOKEN","access-token"},
            {"NETWORK_AUTHORISED_ACCOUNTS","@instantiator@mastodon.social,@icgames@botsin.space"},
            {"NETWORK_DRY_RUNS","true"},
        };
    }
}

