using System;
using ConsensusChessFeatureTests.Data;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
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
                    p.Type == PostType.CommandRejection &&
					p.NetworkReplyToId == command.SourcePostId),
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

			WaitAndAssert(() =>
			{
				using (var db = Dbo.GetDb())
					return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
			});
			using (var db = Dbo.GetDb())
			{
				Assert.AreEqual(1, db.Games.Single().CurrentBoard.BoardPosts.Count());
                Assert.IsTrue(db.Games.Single().CurrentBoard.BoardPosts.Single().Succeeded);
				Assert.IsTrue(db.Games.Single().CurrentBoard.BoardPosts.Single().Type == PostType.Node_BoardUpdate);
            }

            var confirmation = $"New board. You have {game.MoveDuration.ToString()} to vote.";
            WaitAndAssert(() => postsSent.Count(p => p.Message.StartsWith(confirmation)) == 1);
            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Message.StartsWith(confirmation)),
                null),
                Times.Once);
        }

        [TestMethod]
        public async Task ValidVote_is_AcceptedAndStored()
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

            WaitAndAssert(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
            });

            var confirmation = $"New board. You have {game.MoveDuration.ToString()} to vote.";
            WaitAndAssert(() => postsSent.Count(p => p.Message.StartsWith(confirmation)) == 1);
            var boardPost = postsSent.Single(p => p.Message.StartsWith(confirmation));

            var command = FeatureDataGenerator.GenerateCommand("move e4", NodeNetwork, inReplyTo: boardPost.NetworkPostId);
            await receivers[NodeId.Shortcode].Invoke(command);

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.MoveAccepted &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e4", vote.MoveText);
                Assert.AreEqual(VoteValidationState.Valid, vote.ValidationState);

                Assert.IsNotNull(vote.Participant);
                Assert.AreEqual(1, vote.Participant.Commitments.Count());
                Assert.AreEqual("instantiator@fake.mastodon.server", vote.Participant.Username.Full);
                Assert.AreEqual(game.Shortcode, vote.Participant.Commitments.Single().GameShortcode);
                Assert.AreEqual(Side.White, vote.Participant.Commitments.Single().GameSide);

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost.Succeeded);
                Assert.AreEqual(PostType.MoveAccepted, vote.ValidationPost.Type);
            }
        }

        [TestMethod]
        public async Task InvalidChessVote_is_RejectedAndStored()
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

            WaitAndAssert(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
            });

            WaitAndAssert(() => postsSent.Count(p => p.Type == PostType.Node_BoardUpdate) == 1);
            var boardPost = postsSent.Single(p => p.Type == PostType.Node_BoardUpdate);

            var command = FeatureDataGenerator.GenerateCommand("move e2 - e5", NodeNetwork, inReplyTo: boardPost.NetworkPostId);
            await receivers[NodeId.Shortcode].Invoke(command);

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.MoveValidation &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e2 - e5", vote.MoveText);
                Assert.AreEqual(VoteValidationState.InvalidMoveText, vote.ValidationState);

                Assert.IsNotNull(vote.Participant);
                Assert.AreEqual(0, vote.Participant.Commitments.Count()); // no commitment created until successful vote

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost.Succeeded);
                Assert.AreEqual(PostType.MoveValidation, vote.ValidationPost.Type);
            }
        }

        [TestMethod]
        public async Task ValidChessVoteForInactiveSide_is_RejectedAndStored()
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

            WaitAndAssert(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
            });

            var confirmation = $"New board. You have {game.MoveDuration.ToString()} to vote.";
            WaitAndAssert(() => postsSent.Count(p => p.Message.StartsWith(confirmation)) == 1);
            var boardPost = postsSent.Single(p => p.Message.StartsWith(confirmation));

            var command = FeatureDataGenerator.GenerateCommand("move e6", NodeNetwork, inReplyTo: boardPost.NetworkPostId);
            await receivers[NodeId.Shortcode].Invoke(command);

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.MoveValidation &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e6", vote.MoveText);
                Assert.AreEqual(VoteValidationState.InvalidMoveText, vote.ValidationState);

                Assert.IsNotNull(vote.Participant);
                Assert.AreEqual(0, vote.Participant.Commitments.Count()); // no commitment created until successful vote

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost.Succeeded);
                Assert.AreEqual(PostType.MoveValidation, vote.ValidationPost.Type);
            }
        }

        [TestMethod]
        public async Task ValidVoteFromOpposingSide_is_RejectedAndStored()
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

            WaitAndAssert(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
            });

            WaitAndAssert(() => postsSent.Count(p => p.Type == PostType.Node_BoardUpdate) == 1);
            var boardPost = postsSent.Single(p => p.Type == PostType.Node_BoardUpdate);

            using (var db = Dbo.GetDb())
            {
                var participant = new Participant(SocialUsername.From("instantiator", "Lewis", NodeNetwork));
                participant.Commitments.Add(
                    new Commitment()
                    {
                        GameShortcode = game.Shortcode,
                        GameSide = Side.Black,
                    });
                db.Participant.Attach(participant);
                db.Participant.Add(participant);
                await db.SaveChangesAsync();
            }

            using (var db = Dbo.GetDb())
            {
                var participant = db.Participant.Single();
                Assert.AreEqual(1, participant.Commitments.Count());
                Assert.AreEqual(SocialUsername.From("instantiator", "Lewis", NodeNetwork), participant.Username);
            }

            var command = FeatureDataGenerator.GenerateCommand("move e2 - e4", NodeNetwork, inReplyTo: boardPost.NetworkPostId);
            await receivers[NodeId.Shortcode].Invoke(command);

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.MoveValidation &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e2 - e4", vote.MoveText);
                Assert.AreEqual(VoteValidationState.OffSide, vote.ValidationState);

                Assert.IsNotNull(vote.Participant);
                Assert.AreEqual(1, vote.Participant.Commitments.Count()); // no additional commitment created

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost.Succeeded);
                Assert.AreEqual(PostType.MoveValidation, vote.ValidationPost.Type);
            }
        }

    }
}

