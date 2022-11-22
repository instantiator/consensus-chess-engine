using System;
using ConsensusChessFeatureTests.Data;
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
					p.Message == "This instruction can't be processed: UnrecognisedCommand" &&
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
				Assert.IsTrue(db.Games.Single().CurrentBoard.BoardPosts.Single().Type == PostType.Node_BoardUpdate);
            }

            var confirmation = $"New board. You have {game.MoveDuration.ToString()} to vote.";
            SpinWait.SpinUntil(() => postsSent.Count(p => p.Message.StartsWith(confirmation)) == 1);
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

            SpinWait.SpinUntil(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
            });

            var confirmation = $"New board. You have {game.MoveDuration.ToString()} to vote.";
            SpinWait.SpinUntil(() => postsSent.Count(p => p.Message.StartsWith(confirmation)) == 1);
            var boardPost = postsSent.Single(p => p.Message.StartsWith(confirmation));

            var command = FeatureDataGenerator.GenerateCommand("move e4", NodeNetwork, inReplyTo: boardPost.NetworkPostId);
            await receivers[NodeId.Shortcode].Invoke(command);

            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Message == "Move accepted - thank you" &&
                    p.NetworkReplyToId == command.SourceId),
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
                Assert.AreEqual("instantiator", vote.Participant.NetworkServer);
                Assert.AreEqual("instantiator", vote.Participant.NetworkUserAccount);
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

            SpinWait.SpinUntil(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
            });

            var confirmation = $"New board. You have {game.MoveDuration.ToString()} to vote.";
            SpinWait.SpinUntil(() => postsSent.Count(p => p.Message.StartsWith(confirmation)) == 1);
            var boardPost = postsSent.Single(p => p.Message.StartsWith(confirmation));

            var command = FeatureDataGenerator.GenerateCommand("move e5", NodeNetwork, inReplyTo: boardPost.NetworkPostId);
            await receivers[NodeId.Shortcode].Invoke(command);

            var validation = "InvalidSAN from instantiator: e5, ChessSanNotFoundException: Given SAN move: e5 has been not found with current board positions.";
            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Message ==  validation &&
                    p.NetworkReplyToId == command.SourceId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e5", vote.MoveText);
                Assert.AreEqual(VoteValidationState.InvalidSAN, vote.ValidationState);

                Assert.IsNotNull(vote.Participant);
                Assert.AreEqual(0, vote.Participant.Commitments.Count()); // no commitment created until successful vote

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost.Succeeded);
                Assert.AreEqual(PostType.MoveValidation, vote.ValidationPost.Type);
                Assert.AreEqual(validation, vote.ValidationPost.Message);
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

            SpinWait.SpinUntil(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
            });

            var confirmation = $"New board. You have {game.MoveDuration.ToString()} to vote.";
            SpinWait.SpinUntil(() => postsSent.Count(p => p.Message.StartsWith(confirmation)) == 1);
            var boardPost = postsSent.Single(p => p.Message.StartsWith(confirmation));

            var command = FeatureDataGenerator.GenerateCommand("move e6", NodeNetwork, inReplyTo: boardPost.NetworkPostId);
            await receivers[NodeId.Shortcode].Invoke(command);

            var validation = "InvalidSAN from instantiator: e6, ChessSanNotFoundException: Given SAN move: e6 has been not found with current board positions.";
            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Message == validation &&
                    p.NetworkReplyToId == command.SourceId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e6", vote.MoveText);
                Assert.AreEqual(VoteValidationState.InvalidSAN, vote.ValidationState);

                Assert.IsNotNull(vote.Participant);
                Assert.AreEqual(0, vote.Participant.Commitments.Count()); // no commitment created until successful vote

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost.Succeeded);
                Assert.AreEqual(PostType.MoveValidation, vote.ValidationPost.Type);
                Assert.AreEqual(validation, vote.ValidationPost.Message);
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

            SpinWait.SpinUntil(() =>
            {
                using (var db = Dbo.GetDb())
                    return db.Games.Single().CurrentBoard.BoardPosts.Count() == 1;
            });

            var confirmation = $"New board. You have {game.MoveDuration.ToString()} to vote.";
            SpinWait.SpinUntil(() => postsSent.Count(p => p.Message.StartsWith(confirmation)) == 1);
            var boardPost = postsSent.Single(p => p.Message.StartsWith(confirmation));

            using (var db = Dbo.GetDb())
            {
                var participant = new Participant()
                {
                    NetworkServer = "instantiator",
                    NetworkUserAccount = "instantiator",
                    Commitments = new List<Commitment>()
                    {
                        new Commitment()
                        {
                            GameShortcode = game.Shortcode,
                            GameSide = Side.Black,
                        }
                    },
                };
                db.Participant.Add(participant);
                await db.SaveChangesAsync();
            }

            var command = FeatureDataGenerator.GenerateCommand("move e4", NodeNetwork, inReplyTo: boardPost.NetworkPostId);
            await receivers[NodeId.Shortcode].Invoke(command);

            var validation = "OffSide from instantiator: e4, ";
            NodeSocialMock.Verify(ns => ns.PostAsync(
                It.Is<Post>(p =>
                    p.Succeeded == true &&
                    p.Message == validation &&
                    p.NetworkReplyToId == command.SourceId),
                null),
                Times.Once);

            using (var db = Dbo.GetDb())
            {
                var move = db.Games.Single().CurrentMove;
                Assert.AreEqual(1, move.Votes.Count());
                var vote = move.Votes.Single();
                Assert.AreEqual("e4", vote.MoveText);
                Assert.AreEqual(VoteValidationState.OffSide, vote.ValidationState);

                Assert.IsNotNull(vote.Participant);
                Assert.AreEqual(1, vote.Participant.Commitments.Count()); // no additional commitment created

                Assert.IsNotNull(vote.ValidationPost);
                Assert.IsTrue(vote.ValidationPost.Succeeded);
                Assert.AreEqual(PostType.MoveValidation, vote.ValidationPost.Type);
                Assert.AreEqual(validation, vote.ValidationPost.Message);
            }
        }

    }
}

