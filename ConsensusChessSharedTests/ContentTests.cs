using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
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
		public void PostsWithBrokenSubstitutions_throws_HandlebarsUndefinedBindingException()
        {
			var post = new PostBuilder(PostType.Engine_GameAnnouncement);
			Assert.ThrowsException<HandlebarsUndefinedBindingException>(() => post.Build());
		}

	}
}

