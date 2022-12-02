using System;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Helpers;
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

