using System;
using ConsensusChessFeatureTests.Data;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;

namespace ConsensusChessFeatureTests
{
	[TestClass]
	public class GameplayTests : AbstractFeatureTest
	{

        [TestMethod]
        public async Task GameRollsIntoCheckmate_resultsIn_GameEndedStatus()
        {
            var engine = await StartEngineAsync();
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync(fen: FeatureDataGenerator.FEN_PreFoolsMate);
            var boardPost = WaitAndAssert_NodePostsBoard(game);

            // add a vote to enact fools mate
            var voteCommand = await ReplyToNodeAsync(boardPost, $"move {FeatureDataGenerator.SAN_FoolsMate}");
            var reply = WaitAndAssert_NodeRepliesTo(voteCommand);
            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(1, db.Games.Single().CurrentMove.Votes.Count());
                WaitAndAssert(() => db.Games.Single().CurrentMove.Votes.Last().ValidationPost != null);
                Assert.AreEqual(VoteValidationState.Valid, db.Games.Single().CurrentMove.Votes.Last().ValidationState);
                Assert.AreEqual("Qf7", db.Games.Single().CurrentMove.Votes.Last().MoveText);
            }

            // expire move and check for the end of the game
            await ExpireCurrentMoveShortlyAsync(game);
            WaitAndAssert_Moves(game, moves: 1, made: 1);
            using (var db = Dbo.GetDb())
            {
                // check the positions and moves
                Assert.AreEqual(1, db.Games.Single().Moves.Count()); // game end does not create an additional Move
                Assert.AreEqual(FeatureDataGenerator.FEN_PreFoolsMate, db.Games.Single().Moves[0].From.FEN);
                Assert.AreEqual(Side.White, db.Games.Single().Moves[0].SideToPlay);
                Assert.AreEqual(Side.White, db.Games.Single().Moves[0].From.ActiveSide);
                Assert.AreEqual("Qxf7#", db.Games.Single().Moves[0].SelectedSAN!);
                Assert.AreEqual(FeatureDataGenerator.FEN_FoolsMate, db.Games.Single().Moves[0].To!.FEN);
                Assert.AreEqual(Side.Black, db.Games.Single().Moves[0].To!.ActiveSide);

                // check game looks ended
                Assert.IsNotNull(db.Games.Single().Finished);
                Assert.AreEqual(1, db.Games.Single().GamePosts.Count(p => p.Type == PostType.Engine_GameEnded));
                Assert.AreEqual(GameState.BlackKingCheckmated, db.Games.Single().State);
                Assert.IsFalse(db.Games.Single().Active);
            }
        }

        [TestMethod]
		public async Task FullGame_withValidMoves_FollowsHappyPathToCompletion()
		{
            var engine = await StartEngineAsync();
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();

            // game ready
            // TODO: not implemented yet

        }

    }
}

