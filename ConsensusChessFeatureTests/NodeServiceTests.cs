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
            var command = await SendToNodeAsync("hello");

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
					p.Succeeded == true &&
                    p.Type == PostType.CommandRejection &&
					p.NetworkReplyToId == command.SourcePostId),
				null),
                Times.Once);
        }

        [TestMethod]
		public async Task NewGame_causes_BoardPostAndInstructions()
		{
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            using (var db = Dbo.GetDb())
            {
                Assert.AreEqual(3, db.Games.Single().CurrentBoard.BoardPosts.Count());
                Assert.IsTrue(db.Games.Single().CurrentBoard.BoardPosts.All(bp => bp.Succeeded));
                Assert.IsTrue(db.Games.Single().CurrentBoard.BoardPosts.Count(bp => bp.Type == PostType.Node_BoardUpdate) == 1);
                Assert.IsTrue(db.Games.Single().CurrentBoard.BoardPosts.Count(bp => bp.Type == PostType.Node_VotingInstructions) == 1);
                Assert.IsTrue(db.Games.Single().CurrentBoard.BoardPosts.Count(bp => bp.Type == PostType.Node_FollowInstructions) == 1);
            }

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_BoardUpdate),
                null),
                Times.Once);

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_VotingInstructions),
                null),
                Times.Once);
        }

        [TestMethod]
        public async Task AbandonedGame_causes_GameAbandonedPost()
        {
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            using (var db = Dbo.GetDb())
            {
                db.Games.Single().State = GameState.Abandoned;
                await db.SaveChangesAsync();
            }

            WaitAndAssert(() =>
                postsSent.Count(p => p.Type == PostType.Node_GameAbandonedUpdate) == 1);

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_GameAbandonedUpdate),
                null),
                Times.Once);
        }

        [TestMethod]
        public async Task ValidVoteSAN_is_AcceptedAndStored()
        {
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            var boardPost = WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            var command = await ReplyToNodeAsync(boardPost, "move e4");

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_VoteAccepted &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e4", vote.MoveText);
                Assert.AreEqual("e4", vote.MoveSAN);
                Assert.AreEqual(VoteValidationState.Valid, vote.ValidationState);

                Assert.IsNotNull(vote.Participant);
                Assert.AreEqual(1, vote.Participant.Commitments.Count());
                Assert.AreEqual("instantiator@fake.mastodon.server", vote.Participant.Username.Full);
                Assert.AreEqual(game.Shortcode, vote.Participant.Commitments.Single().GameShortcode);
                Assert.AreEqual(Side.White, vote.Participant.Commitments.Single().GameSide);

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost.Succeeded);
                Assert.AreEqual(PostType.Node_VoteAccepted, vote.ValidationPost.Type);
            }
        }

        [TestMethod]
        public async Task ValidVoteCCF_is_AcceptedAndStored()
        {
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            var boardPost = WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            var command = await ReplyToNodeAsync(boardPost, "move e2 - e4");

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_VoteAccepted &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e2 - e4", vote.MoveText);
                Assert.AreEqual("e4", vote.MoveSAN);
                Assert.AreEqual(VoteValidationState.Valid, vote.ValidationState);
            }
        }

        [TestMethod]
        public async Task ValidVoteCCF_WithoutPrefix_is_AcceptedAndStored()
        {
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            var boardPost = WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            var command = await ReplyToNodeAsync(boardPost, "e2 - e4");

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_VoteAccepted &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e2 - e4", vote.MoveText);
                Assert.AreEqual("e4", vote.MoveSAN);
                Assert.AreEqual(VoteValidationState.Valid, vote.ValidationState);
            }
        }

        [TestMethod]
        public async Task ValidVoteSAN_WithoutPrefix_is_AcceptedAndStored()
        {
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            var boardPost = WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            var command = await ReplyToNodeAsync(boardPost, "e4");

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_VoteAccepted &&
                    p.NetworkReplyToId == command.SourcePostId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e4", vote.MoveText);
                Assert.AreEqual("e4", vote.MoveSAN);
                Assert.AreEqual(VoteValidationState.Valid, vote.ValidationState);
            }
        }


        [TestMethod]
        public async Task ValidVote_afterValidVode_AcceptedAndStoredAndSupercedesPreviousVote()
        {
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            var boardPost = WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            var command1 = await ReplyToNodeAsync(boardPost, "e2 - e4");

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_VoteAccepted &&
                    p.NetworkReplyToId == command1.SourcePostId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e2 - e4", vote.MoveText);
                Assert.AreEqual("e4", vote.MoveSAN);
                Assert.AreEqual(VoteValidationState.Valid, vote.ValidationState);
            }

            var command2 = await ReplyToNodeAsync(boardPost, "f2 - f4");

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_VoteAccepted &&
                    p.NetworkReplyToId == command2.SourcePostId),
                null),
                Times.Once);

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_VoteSuperceded &&
                    p.NetworkReplyToId == command1.SourcePostId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(2, move.Votes.Count());
                var oldVote = move.Votes.Single(v => v.ValidationState == VoteValidationState.Superceded);
                var newVote = move.Votes.Single(v => v.ValidationState == VoteValidationState.Valid);
                Assert.AreEqual("f2 - f4", newVote.MoveText);
                Assert.AreEqual("f4", newVote.MoveSAN);
            }
        }

        [TestMethod]
        public async Task InvalidChessVote_is_RejectedAndStored()
        {
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            var boardPost = WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            var command = await ReplyToNodeAsync(boardPost, "move e2 - e5");

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_MoveValidation &&
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
                Assert.AreEqual(PostType.Node_MoveValidation, vote.ValidationPost.Type);
            }
        }

        [TestMethod]
        public async Task ValidChessVoteForInactiveSide_is_RejectedAndStored()
        {
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            var boardPost = WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            var command = await ReplyToNodeAsync(boardPost, "move e6");

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_MoveValidation &&
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
                Assert.AreEqual(PostType.Node_MoveValidation, vote.ValidationPost.Type);
            }
        }

        [TestMethod]
        public async Task ValidVoteFromOpposingSide_is_RejectedAndStored()
        {
            var node = await StartNodeAsync();
            var game = await StartGameWithDbAsync();
            var boardPost = WaitAndAssert_NodePostsBoard(game, NodeId.Shortcode);

            // create a participant record for the wrong side
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

            // check participant record is retrievable
            using (var db = Dbo.GetDb())
            {
                var participant = db.Participant.Single();
                Assert.AreEqual(1, participant.Commitments.Count());
                Assert.AreEqual(SocialUsername.From("instantiator", "Lewis", NodeNetwork), participant.Username);
            }

            var command = await ReplyToNodeAsync(boardPost, "move e2 - e4");

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Type == PostType.Node_MoveValidation &&
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
                Assert.AreEqual(1, vote.Participant.Commitments.Count(), "There should not be another participant commitment");

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost.Succeeded);
                Assert.AreEqual(PostType.Node_MoveValidation, vote.ValidationPost.Type);
            }
        }

    }
}

