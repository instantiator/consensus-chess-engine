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

            var command = FeatureDataGenerator.GenerateCommand("hello", NodeNetwork);

            await receivers[NodeId.Shortcode].Invoke(command);

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
					p.Succeeded == true &&
					p.Message == "UnrecognisedCommand" &&
					p.NetworkReplyToId == command.SourceId),
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

