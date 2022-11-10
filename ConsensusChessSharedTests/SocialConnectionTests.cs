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
            network = Network.FromEnvironment(SampleDataGenerator.SimpleNetworkEnv);
            state = SampleDataGenerator.NodeState;
        }

        [TestMethod]
        public void SocialFactory_creates_MastodonConnection()
		{
            var connection = SocialFactory.From(mockLogger.Object, network, state);
            var mastodonConnection = connection as MastodonConnection;
            Assert.IsNotNull(mastodonConnection);

            // would ordinarily run InitAsync
            // can't check CalculateSkips - it uses the account
		}
	}
}

