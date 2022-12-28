using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Helpers;
using ConsensusChessShared.Social;
using ConsensusChessSharedTests.Data;
using HandlebarsDotNet.Compiler;
using static ConsensusChessShared.Graphics.BoardGraphicsData;

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
        public void AllEnumsHaveTranslations()
        {
            var translator = new EnumTranslator();
            TestEnum((SocialStatus x) => translator.Translate(x));
            TestEnum((GameState x) => translator.Translate(x));
            TestEnum((SideRules x) => translator.Translate(x));
            TestEnum((SideRules x) => translator.Explain(x));
            TestEnum((VoteValidationState x) => translator.Describe(x));
            TestEnum((CommandRejectionReason x) => translator.Describe(x));
            TestEnum((GameNotFoundReason x) => translator.Describe(x));
        }

        private void TestEnum<T>(Func<T,string> translation)
        {
            foreach (T enumeration in Enum.GetValues(typeof(T)))
            {
                Assert.IsFalse(
                    string.IsNullOrWhiteSpace(translation(enumeration)),
                    $"{typeof(T).GetType().Name}.{enumeration} does not have a translation.");
            }
        }

        [TestMethod]
		public void PostsWithBrokenSubstitutions_throws_HandlebarsUndefinedBindingException()
        {
            var posts = SampleDataGenerator.PostBuilderFactory;
			var post = posts.Create(PostType.Engine_GameAnnouncement);
			Assert.ThrowsException<HandlebarsUndefinedBindingException>(() => post.Build());
		}


        [TestMethod]
        public void NodeBoardUpdateTest()
        {
            var game = SampleDataGenerator.SimpleMoveLockGame();
            var board = game.CurrentBoard;
            var posts = SampleDataGenerator.PostBuilderFactory;
            var post = posts.Node_BoardUpdate(
                game, board,
                BoardFormatter.BoardFormat.StandardFAN,
                BoardStyle.PixelChess)
                    .Build();

            Assert.IsNotNull(post);
            Assert.IsTrue(post.Message!.Contains(game.Title));
            Assert.AreEqual(1, post.Media.Count());
            Assert.IsFalse(post.Media[0].Alt.Contains("&#9820;"));
            Assert.IsTrue(post.Media[0].Alt.Contains("♜"));
        }

        [TestMethod]
        public void CanRestoreUnicode()
        {
            var unicodeString = "black rook &#9820;";

            Assert.IsTrue(unicodeString.Contains("&#9820;"));
            Assert.IsFalse(unicodeString.RestoreUnicode()!.Contains("&#9820;"));
            Assert.IsTrue(unicodeString.RestoreUnicode()!.Contains("♜"));
        }
    }
}

