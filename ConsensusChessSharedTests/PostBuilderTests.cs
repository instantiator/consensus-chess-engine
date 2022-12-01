using System;
using ConsensusChessShared.Content;
using ConsensusChessSharedTests.Data;
using static ConsensusChessShared.Content.BoardGraphicsData;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class PostBuilderTests
	{

		[TestMethod]
		public void NodeBoardUpdateTest()
		{
			var game = SampleDataGenerator.SimpleMoveLockGame();
			var board = game.CurrentBoard;
			var post = PostBuilder.Node_BoardUpdate(
				game, board,
				BoardFormatter.BoardFormat.StandardFAN,
				BoardStyle.PixelChess)
					.Build();

			Assert.IsNotNull(post);
			Assert.IsTrue(post.Message!.Contains(game.Title));
			Assert.AreEqual(1, post.Media.Count());
		}

	}
}

