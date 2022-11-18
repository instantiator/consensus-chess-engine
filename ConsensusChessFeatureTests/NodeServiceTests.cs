using System;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;
using Moq;

namespace ConsensusChessFeatureTests
{
	[TestClass]
	public class NodeServiceTests : AbstractFeatureTest
	{
		[TestMethod]
		public async Task GarbageInGarbageOut()
		{
			var node = await StartNodeAsync();

            NodeSocialMock.Verify(ns => ns.StartListeningForCommandsAsync(
				It.IsAny<Func<SocialCommand, Task>>(),
				It.IsAny<bool>()),
				Times.Once);

            var command = new SocialCommand()
			{
				DeliveryMedium = "mock",
				DeliveryType = "notification",
				IsAuthorised = true,
				IsRetrospective = false,
				NetworkUserId = "instantiator",
				IsForThisNode = true,
				RawText = "hello",
				ReceivingNetwork = NodeNetwork,
				SourceAccount = "instantiator",
				SourceId = postId++
			};

			await receivers[NodeId.Shortcode].Invoke(command);

            NodeSocialMock.Verify(ns => ns.ReplyAsync(
				command,
				"UnrecognisedCommand",
				PostType.CommandResponse,
				null),
				Times.Once);
		}

	}
}

