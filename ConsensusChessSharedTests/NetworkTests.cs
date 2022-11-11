﻿using System;
using ConsensusChessShared.DTO;
using ConsensusChessSharedTests.Data;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class NetworkTests
	{
		[TestMethod]
		public void Network_creates_FromEnvironment()
		{
			var network = Network.FromEnvironment(SampleDataGenerator.SimpleNetworkEnv);

			Assert.AreEqual(NetworkType.Mastodon, network.Type);
			Assert.AreEqual("some.kind.of.mastodon", network.NetworkServer);
			Assert.AreEqual("app-name",network.AppName);
            Assert.AreEqual("app-key",network.AppKey);
            Assert.AreEqual("app-secret",network.AppSecret);
            Assert.AreEqual("access-token",network.AppToken);
            Assert.AreEqual("@instantiator@mastodon.social,@icgames@botsin.space", network.AuthorisedAccounts);
            Assert.AreEqual(2, network.AuthorisedAccountsList.Count());
            Assert.AreEqual("instantiator@mastodon.social", network.AuthorisedAccountsList.ElementAt(0));
            Assert.AreEqual("icgames@botsin.space", network.AuthorisedAccountsList.ElementAt(1));
            Assert.IsTrue(network.DryRuns);
        }

        /*
        {"NETWORK_TYPE","Mastodon"},
        {"NETWORK_SERVER","some.kind.of.mastodon"},
        {"NETWORK_APP_NAME","app-name"},
        {"NETWORK_APP_KEY","app-key"},
        {"NETWORK_APP_SECRET","app-secret"},
        {"NETWORK_ACCESS_TOKEN","access-token"},
        {"NETWORK_AUTHORISED_ACCOUNTS","@instantiator@mastodon.social,@icgames@botsin.space"},
        */
    }
}
