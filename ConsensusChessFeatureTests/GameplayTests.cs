using System;
using Chess;
using ConsensusChessFeatureTests.Data;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using static ConsensusChessFeatureTests.Data.GameplayTestData;

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
            var game = await StartGameWithDbAsync(fen: FeatureDataGenerator.FEN_PreSimpleCheckmate);
            var boardPost = WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            // add a vote to enact fools mate
            var voteCommand = await ReplyToNodeAsync(boardPost, $"move {FeatureDataGenerator.SAN_SimpleCheckmate}");
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
                Assert.AreEqual(FeatureDataGenerator.FEN_PreSimpleCheckmate, db.Games.Single().Moves[0].From.FEN);
                Assert.AreEqual(Side.White, db.Games.Single().Moves[0].SideToPlay);
                Assert.AreEqual(Side.White, db.Games.Single().Moves[0].From.ActiveSide);
                Assert.AreEqual("Qxf7#", db.Games.Single().Moves[0].SelectedSAN!);
                Assert.AreEqual(FeatureDataGenerator.FEN_SimpleCheckmate, db.Games.Single().Moves[0].To!.FEN);
                Assert.AreEqual(Side.Black, db.Games.Single().Moves[0].To!.ActiveSide);

                // check game looks ended
                Assert.IsNotNull(db.Games.Single().Finished);
                Assert.AreEqual(1, db.Games.Single().GamePosts.Count(p => p.Type == PostType.Engine_GameEnded));
                Assert.AreEqual(GameState.BlackKingCheckmated, db.Games.Single().State);
                Assert.IsFalse(db.Games.Single().Active);
            }

            WaitAndAssert_Post(shortcode: EngineId.Shortcode, ofType: PostType.Engine_GameEnded);
            WaitAndAssert_Post(shortcode: NodeId.Shortcode, ofType: PostType.Node_GameEndedUpdate);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetGamesData), DynamicDataSourceType.Method)]
        public async Task FullGame(GameplayTestData.StartingPosition position, GameplayTestData.Reenactment reenactment, Prefix prefixRule, GameState endGameState)
        {
            var engine = await StartEngineAsync();
            var node = await StartNodeAsync();

            var fen = GameplayTestData.FEN_positions[position];
            var moves = GameplayTestData.MOVE_sequences[reenactment];

            var startCommand = await SendToEngineAsync($"new {NodeId.Shortcode}");

            WaitAndAssert_Post(
                shortcode: EngineId.Shortcode,
                ofType: PostType.Engine_GameAnnouncement);

            Game game;
            using (var db = Dbo.GetDb())
                game = db.Games.Single();

            var whiteToPlay = true;
            var boardCount = 1;
            foreach (var move in moves)
            {
                WriteLogLine($"⏳ Waiting for board post #{boardCount}...");

                var boardPost = WaitAndAssert_Posts(
                    shortcode: NodeId.Shortcode,
                    ofType: PostType.Node_BoardUpdate,
                    count: boardCount)
                        .Last();

                WaitAndAssert_Posts(
                    shortcode: NodeId.Shortcode,
                    ofType: PostType.Node_VotingInstructions,
                    count: boardCount * 2 - 1)
                        .Last();

                WaitAndAssert_Posts(
                    shortcode: NodeId.Shortcode,
                    ofType: PostType.Node_FollowInstructions,
                    count: boardCount * 2 - 1)
                        .Last();

                WriteLogLine($"✅ Board post #{boardCount} received.");

                // make sure the database has caught up
                WaitAndAssert(() =>
                {
                    using (var db = Dbo.GetDb())
                    {
                        return
                            db.Games.Single().Moves.Count() == boardCount &&
                            db.Games.Single().CurrentBoard.BoardPosts.Count() == 3;
                    }
                });

                var player = whiteToPlay ? "player-white" : "player-black";

                var prefix = prefixRule == Prefix.NoPrefix
                    ? string.Empty
                    : $"{prefixRule.ToString()} ";

                var movePost = await ReplyToNodeAsync(
                    boardPost: boardPost,
                    text: $"{prefix}{move}",
                    from: player);

                WriteLogLine($"✅ Posted move: {move}");
                WriteLogLine($"⏳ Waiting for validation response to move: {move}...");

                // capture the node's response

                // use this code to capture the fine detail of the response
                // var criteria = () =>
                // {
                //     return postsSent.Any(p
                //         => p.Succeeded && p.NetworkReplyToId == movePost.SourcePostId);
                // };
                // SpinWait.SpinUntil(criteria, spinWaitTimeout);
                // var found = postsSent.Where(p
                //         => p.Succeeded && p.NetworkReplyToId == movePost.SourcePostId);
                // Assert.AreEqual(1, found.Count());
                // WriteLogLine($"➡️ Post found: {found.Single().Type}");
                // if (found.Single().Type != PostType.MoveAccepted)
                //     WriteLogLine($"➡️ Message: {found.Single().Message}");
                // Assert.IsTrue(found.Single().Type == PostType.MoveAccepted);

                var validationPost = WaitAndAssert_NodeRepliesTo(
                    command: movePost,
                    ofType: PostType.Node_VoteAccepted);

                await ExpireCurrentMoveShortlyAsync(game, TimeSpan.FromMinutes(30));

                WaitAndAssert_Posts(
                    shortcode: NodeId.Shortcode,
                    ofType: PostType.Node_BoardReminder,
                    count: boardCount)
                        .Last();

                WaitAndAssert_Posts(
                    shortcode: NodeId.Shortcode,
                    ofType: PostType.Node_VotingInstructions,
                    count: boardCount * 2)
                        .Last();

                WaitAndAssert_Posts(
                    shortcode: NodeId.Shortcode,
                    ofType: PostType.Node_FollowInstructions,
                    count: boardCount * 2)
                        .Last();

                await ExpireCurrentMoveShortlyAsync(game);

                whiteToPlay = !whiteToPlay;
                boardCount++;
            }

            WriteLogLine($"⏳ Waiting for engine end game post...");
            WaitAndAssert_Post(
                shortcode: EngineId.Shortcode,
                ofType: PostType.Engine_GameEnded);
            WriteLogLine($"✅ Engine end game post received.");

            WriteLogLine($"⏳ Waiting for node end game post...");
            WaitAndAssert_Post(
                shortcode: NodeId.Shortcode,
                ofType: PostType.Node_GameEndedUpdate);
            WriteLogLine($"✅ Node end game post received.");

            WaitAndAssert(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().State == endGameState;
            });
            WriteLogLine($"✅ Game in state: {endGameState}");
        }

        public static IEnumerable<object[]> GetGamesData()
        {
            return new List<object[]>()
            {
                new object[]
                {
                    StartingPosition.Standard,
                    Reenactment.FoolsMateSAN,
                    Prefix.NoPrefix,
                    GameState.WhiteKingCheckmated
                },
                new object[]
                {
                    StartingPosition.Standard,
                    Reenactment.FoolsMateCCF,
                    Prefix.NoPrefix,
                    GameState.WhiteKingCheckmated
                },
                new object[]
                {
                    StartingPosition.Standard,
                    Reenactment.FoolsMateSAN,
                    Prefix.Move,
                    GameState.WhiteKingCheckmated
                },
                new object[]
                {
                    StartingPosition.Standard,
                    Reenactment.FoolsMateCCF,
                    Prefix.Move,
                    GameState.WhiteKingCheckmated
                },
            };
        }


    }
}

