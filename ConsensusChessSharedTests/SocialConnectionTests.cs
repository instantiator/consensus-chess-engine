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

        private Mock<ILogger> mockLogger;
        private Network network;
        private NodeState state;

        [TestInitialize]
        public void Init()
        {
            mockLogger = new Mock<ILogger>();
            network = Network.FromEnv(SampleDataGenerator.SimpleNetworkEnv);
            state = SampleDataGenerator.NodeState;
        }

        [TestMethod]
        public async Task SocialFactory_creates_MastodonConnection()
		{
            var connection = SocialFactory.From(mockLogger.Object, network);
            var mastodonConnection = connection as MastodonConnection;
            Assert.IsNotNull(mastodonConnection);
            // would ordinarily run InitAsync, requires state, which depends on network
            // await mastodonConnection.InitAsync(state);
            // var skips = mastodonConnection.CalculateCommandSkips();
		}
	}
}

