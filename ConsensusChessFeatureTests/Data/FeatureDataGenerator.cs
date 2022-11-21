﻿using System;
using ConsensusChessFeatureTests.Service;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConsensusChessFeatureTests.Data
{
	public class FeatureDataGenerator
	{
        public static long RollingPostId { get; set; } = 1;

        public static Dictionary<string, string> NodeEnv =>
            new Dictionary<string, string>()
            {
                    { "NODE_NAME", "a fake node" },
                    { "NODE_SHORTCODE", "node-0-fake" },
                    { "NETWORK_TYPE", "Mastodon" },
                    { "NETWORK_SERVER", "fake.mastodon.server" },
                    { "NETWORK_APP_NAME", "node-app" },
                    { "NETWORK_APP_KEY", "app-key" },
                    { "NETWORK_APP_SECRET", "app-secret" },
                    { "NETWORK_ACCESS_TOKEN", "access-token" },
                    { "NETWORK_AUTHORISED_ACCOUNTS", "engine" },
                    { "NETWORK_DRY_RUNS", "false" }
            };

        public static Dictionary<string, string> EngineEnv =>
            new Dictionary<string, string>()
            {
                    { "NODE_NAME", "a fake engine" },
                    { "NODE_SHORTCODE", "engine-fake" },
                    { "NETWORK_TYPE", "Mastodon" },
                    { "NETWORK_SERVER", "fake.mastodon.server" },
                    { "NETWORK_APP_NAME", "engine-app" },
                    { "NETWORK_APP_KEY", "app-key" },
                    { "NETWORK_APP_SECRET", "app-secret" },
                    { "NETWORK_ACCESS_TOKEN", "access-token" },
                    { "NETWORK_AUTHORISED_ACCOUNTS", "admin@chesstodon.social" },
                    { "NETWORK_DRY_RUNS", "false" }
            };

        public static Post SimulatePost(Post post, string shortcode, Network network)
        {
            post.Attempted = DateTime.Now.ToUniversalTime();
            post.AppName = network.AppName;
            post.NetworkServer = network.NetworkServer;
            post.NodeShortcode = shortcode;
            post.Succeeded = true;
            post.NetworkPostId = RollingPostId++;
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
                NetworkPostId = RollingPostId++,
            };
        }
    }
}

