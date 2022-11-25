using System;
using ConsensusChessEngine.Service;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;
using ConsensusChessSharedTests.Data;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class SocialConnectionTests
	{

        private Mock<ILogger>? mockLogger;
        private Network? network;

        [TestInitialize]
        public void Init()
        {
            mockLogger = new Mock<ILogger>();
            network = Network.FromEnv(SampleDataGenerator.SimpleNetworkEnv);
        }

        [TestMethod]
        public async Task SocialFactory_creates_MastodonConnection()
		{
            var connection = SocialFactory.From(mockLogger!.Object, network!, "some-short-code");
            var mastodonConnection = connection as MastodonConnection;
            Assert.IsNotNull(mastodonConnection);
            Assert.IsFalse(mastodonConnection.Ready);
            // InitAsync has not been run - save it for integration tests
		}
	}
}

