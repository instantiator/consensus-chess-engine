using System;
using ConsensusChessShared.Constants;
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
			var network = Network.FromEnv(SampleDataGenerator.SimpleNetworkEnv);

			Assert.AreEqual(NetworkType.Mastodon, network.Type);
			Assert.AreEqual("some.kind.of.mastodon", network.NetworkServer);
			Assert.AreEqual("app-name",network.AppName);
            Assert.AreEqual("access-token",network.AppToken);
            Assert.AreEqual("instantiator@mastodon.social,icgames@botsin.space", network.AuthorisedAccounts);
            Assert.AreEqual(2, network.AuthorisedAccountsList.Count());
            Assert.AreEqual("instantiator@mastodon.social", network.AuthorisedAccountsList.ElementAt(0));
            Assert.AreEqual("icgames@botsin.space", network.AuthorisedAccountsList.ElementAt(1));
            Assert.IsTrue(network.DryRuns);
        }
    }
}

