using System;
using ConsensusChessFeatureTests.Data;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;
using Moq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ConsensusChessFeatureTests
{
	[TestClass]
	public class NodeServiceTests : AbstractFeatureTest
	{
		[TestMethod]
		public async Task GarbageIn_GarbageOut()
		{
			var node = await StartNodeAsync();

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
                SourceId = FeatureDataGenerator.RollingPostId++
            };

			await receivers[NodeId.Shortcode].Invoke(command);

            NodeSocialMock.Verify(ns => ns.ReplyAsync(
				command,
				"UnrecognisedCommand",
				PostType.CommandResponse,
				null),
				Times.Once);
		}

		[TestMethod]
		public async Task NewGame_causes_BoardPost()
		{
            var node = await StartNodeAsync();

			var game = Game.NewGame("game-shortcode", "description",
					new[] { NodeNetwork.NetworkServer },
					new[] { NodeNetwork.NetworkServer },
					new[] { NodeId.Shortcode },
					new[] { NodeId.Shortcode },
					SideRules.MoveLock);

            using (var db = Dbo.GetDb())
			{
				db.Games.Add(game);
				await db.SaveChangesAsync();
			}

			SpinWait.SpinUntil(() =>
			{
				using (var db = Dbo.GetDb())
					return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
			});
			using (var db = Dbo.GetDb())
			{
				Assert.AreEqual(1, db.Games.Single().CurrentBoard.BoardPosts.Count());
                Assert.IsTrue(db.Games.Single().CurrentBoard.BoardPosts.Single().Succeeded);
				Assert.IsTrue(db.Games.Single().CurrentBoard.BoardPosts.Single().Type == PostType.BoardUpdate);
            }
        }

    }
}

