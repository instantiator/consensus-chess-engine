using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.Social;
using HandlebarsDotNet.Compiler;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class ContentTests
	{
		[TestMethod]
		public void AllPostTypesHaveTemplates()
		{
			foreach (var type in Enum.GetValues<PostType>())
			{
				Assert.IsTrue(
					PostTemplates.TemplateSource.ContainsKey(type),
					$"PostType.{type} does not have a template.");
			}
		}

        [TestMethod]
        public void AllBoardDescriptionTypesHaveTemplates()
        {
            foreach (var type in Enum.GetValues<DescriptionType>())
            {
                Assert.IsTrue(
                    BoardTemplates.TemplateSource.ContainsKey(type),
                    $"DescriptionType.{type} does not have a template.");
            }
        }

        [TestMethod]
        public void AllPostTemplatesAreWithinLimit()
        {
            var limit = 350;
            foreach (var type in Enum.GetValues<PostType>())
            {
                Assert.IsTrue(
                    PostTemplates.TemplateSource[type].Length < limit,
                    $"PostType.{type} has {PostTemplates.TemplateSource[type].Length} characters. Limit is: {limit}");
            }
        }

        [TestMethod]
        public void AllPostTypesHaveMastodonVisibility()
        {
            foreach (var type in Enum.GetValues<PostType>())
            {
                Assert.IsTrue(
                    MastodonConnection.VisibilityMapping.ContainsKey(type),
                    $"PostType.{type} does not have a visibility set for Mastodon.");
            }
        }

        [TestMethod]
		public void PostsWithBrokenSubstitutions_throws_HandlebarsUndefinedBindingException()
        {
			var post = PostBuilder.Create(PostType.Engine_GameAnnouncement);
			Assert.ThrowsException<HandlebarsUndefinedBindingException>(() => post.Build());
		}

	}
}

