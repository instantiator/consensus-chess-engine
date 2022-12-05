using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using ConsensusChessSharedTests.Data;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class PostBuilderFactoryTests
	{
        [TestMethod]
        public void SocialStatus_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).SocialStatus(
                SampleDataGenerator.NodeState,
                SocialStatus.Started);
            var post = builder.Build();
        }

        [TestMethod]
        public void CommandRejection_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).CommandRejection(
                CommandRejectionReason.UnrecognisedCommand);
            var post = builder.Build();
        }

        [TestMethod]
        public void CommandResponse_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).CommandResponse(
                "blah");
            var post = builder.Build();
        }

        [TestMethod]
        public void Node_VoteAccepted_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Node_MoveAccepted(
                SampleDataGenerator.SimpleMoveLockGame().CurrentMove,
                SampleDataGenerator.SimpleMoveLockGame(),
                SampleDataGenerator.SimpleMoveLockGame().CurrentSide,
                SampleDataGenerator.SampleVote());
            var post = builder.Build();
        }

        [TestMethod]
        public void Node_VoteSuperceded_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Node_MoveSuperceded(
                SampleDataGenerator.SimpleMoveLockGame().CurrentMove,
                SampleDataGenerator.SimpleMoveLockGame(),
                SampleDataGenerator.SimpleMoveLockGame().CurrentSide,
                SampleDataGenerator.SampleVote(),
                SampleDataGenerator.SampleVote());
            var post = builder.Build();
        }

        [TestMethod]
        public void Node_GameNotFound_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Node_GameNotFound(
                GameNotFoundReason.NoLinkedGame);
            var post = builder.Build();
        }

        [TestMethod]
        public void Node_MoveValidation_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Node_MoveValidation(
                SampleDataGenerator.SimpleMoveLockGame().CurrentMove,
                VoteValidationState.InvalidMoveText,
                SampleDataGenerator.SampleUsername(),
                "ab - cd",
                "it's rubbish");
            var post = builder.Build();
        }

        [TestMethod]
        public void Engine_GameCreationResponse_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Engine_GameCreationResponse(
                SampleDataGenerator.SimpleMoveLockGame());
            var post = builder.Build();
        }

        [TestMethod]
        public void Engine_GameAnnouncement_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Engine_GameAnnouncement(
                SampleDataGenerator.SimpleMoveLockGame());
            var post = builder.Build();
        }

        [TestMethod]
        public void Engine_GameAdvance_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Engine_GameAdvance(
                SampleDataGenerator.SimpleMoveLockGame());
            var post = builder.Build();
        }

        [TestMethod]
        public void Engine_GameAbandoned_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Engine_GameAbandoned(
                SampleDataGenerator.SimpleMoveLockGame());
            var post = builder.Build();
        }

        [TestMethod]
        public void Engine_GameEnded_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Engine_GameEnded(
                SampleDataGenerator.SimpleMoveLockGame());
            var post = builder.Build();
        }

        [TestMethod]
        public void Node_BoardUpdate_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Node_BoardUpdate(
                SampleDataGenerator.SimpleMoveLockGame(),
                SampleDataGenerator.SimpleMoveLockGame().CurrentBoard,
                BoardFormatter.BoardFormat.StandardFAN,
                BoardGraphicsData.BoardStyle.PixelChess);
            var post = builder.Build();
        }

        [TestMethod]
        public void Node_BoardReminder_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Node_BoardReminder(
                SampleDataGenerator.SimpleMoveLockGame(),
                SampleDataGenerator.SimpleMoveLockGame().CurrentBoard,
                SampleDataGenerator.SimpleMoveLockGame().CurrentMove,
                BoardFormatter.BoardFormat.StandardFAN,
                BoardGraphicsData.BoardStyle.PixelChess);
            var post = builder.Build();
        }

        [TestMethod]
        public void Node_VotingInstructions_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Node_VotingInstructions();
            var post = builder.Build();
        }

        [TestMethod]
        public void Node_FollowInstructions_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Node_FollowInstructions();
            var post = builder.Build();
        }

        [TestMethod]
        public void Node_GameAbandonedUpdate_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Node_GameAbandonedUpdate(
                SampleDataGenerator.SimpleMoveLockGame(),
                BoardFormatter.BoardFormat.Words_en,
                BoardGraphicsData.BoardStyle.PixelChess);
            var post = builder.Build();
        }

        [TestMethod]
        public void Node_GameEndedUpdate_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Node_GameEndedUpdate(
                SampleDataGenerator.SimpleMoveLockGame(),
                BoardFormatter.BoardFormat.Words_en,
                BoardGraphicsData.BoardStyle.PixelChess);
            var post = builder.Build();
        }

        [TestMethod]
        public void Unspecified_Test()
        {
            var config = ServiceConfig.FromEnv(SampleDataGenerator.SimpleConfig);
            var builder = new PostBuilderFactory(config).Unspecified("urgle burgle");
            var post = builder.Build();
        }
    }
}

