using System;
using System.Runtime.InteropServices.JavaScript;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Mastonet.Entities;
using static ConsensusChessShared.Content.BoardFormatter;
using static ConsensusChessShared.Content.BoardGraphicsData;

namespace ConsensusChessShared.Content
{
	public class PostBuilderFactory
	{
        private ServiceConfig config;

        public PostBuilderFactory(ServiceConfig config)
        {
            this.config = config;
        }

        public PostBuilder Create(PostType type)
            => new PostBuilder(config, type);

        public PostBuilder SocialStatus(NodeState state, SocialStatus status)
            => new PostBuilder(config, PostType.SocialStatus)
                .WithNodeState(state)
                .WithSocialStatus(status);

        public PostBuilder Engine_GameCreationResponse(Game game)
            => new PostBuilder(config, PostType.Engine_GameCreationResponse)
                .WithGame(game);

        public PostBuilder Engine_GameAnnouncement(Game game)
            => new PostBuilder(config, PostType.Engine_GameAnnouncement)
                .WithGame(game);

        public PostBuilder Engine_GameAbandoned(Game game)
            => new PostBuilder(config, PostType.Engine_GameAbandoned)
                .WithGame(game);

        public PostBuilder Engine_GameEnded(Game game)
            => new PostBuilder(config, PostType.Engine_GameEnded)
                .WithGame(game);

        public PostBuilder Engine_GameAdvance(Game game)
            => new PostBuilder(config, PostType.Engine_GameAdvance)
                .WithGame(game);

        public PostBuilder Node_BoardUpdate(Game game, Board board, BoardFormat format, BoardStyle style)
            => new PostBuilder(config, PostType.Node_BoardUpdate)
                .WithGame(game)
                .WithBoard(board, format)
                .AndBoardGraphic(style, format);

        public PostBuilder Node_BoardReminder(Game game, Board board, Move move, BoardFormat format, BoardStyle style)
            => new PostBuilder(config, PostType.Node_BoardReminder)
                .WithGame(game)
                .WithBoard(board, format)
                .WithMove(move)
                .AndBoardGraphic(style, format);

        public PostBuilder Node_VotingInstructions()
            => new PostBuilder(config, PostType.Node_VotingInstructions);

        public PostBuilder Node_FollowInstructions()
            => new PostBuilder(config, PostType.Node_FollowInstructions);

        public PostBuilder Node_GameAbandonedUpdate(Game game, BoardFormat format, BoardStyle style)
            => new PostBuilder(config, PostType.Node_GameAbandonedUpdate)
                .WithGame(game)
                .WithBoard(game.CurrentBoard, format)
                .AndBoardGraphic(style, format);

        public PostBuilder Node_GameEndedUpdate(Game game, BoardFormat format, BoardStyle style)
            => new PostBuilder(config, PostType.Node_GameEndedUpdate)
                .WithGame(game)
                .WithBoard(game.CurrentBoard, format)
                .AndBoardGraphic(style, format);

        public PostBuilder Node_GameNotFound(GameNotFoundReason reason)
            => new PostBuilder(config, PostType.Node_GameNotFound)
                .WithGameNotFoundReason(reason);

        public PostBuilder Node_MoveAccepted(Move move, Game game, Side side, Vote vote)
            => new PostBuilder(config, PostType.Node_VoteAccepted)
                .WithMove(move)
                .WithGame(game)
                .WithSide(side)
                .WithVote(vote);

        public PostBuilder Node_MoveSuperceded(Move move, Game game, Side side, Vote preexistingVote, Vote vote)
            => new PostBuilder(config, PostType.Node_VoteSuperceded)
                .WithMove(move)
                .WithGame(game)
                .WithSide(side)
                .WithPreexistingVote(preexistingVote)
                .WithVote(vote);

        public PostBuilder Node_MoveValidation(Move move, VoteValidationState state, SocialUsername sender, string movetext, string? detail)
            => new PostBuilder(config, PostType.Node_MoveValidation)
                .WithMove(move)
                .WithValidationState(state)
                .WithUsername(sender)
                .WithMoveText(movetext)
                .WithDetail(detail);

        public PostBuilder CommandResponse(string text)
            => new PostBuilder(config, PostType.CommandResponse)
                .WithText(text);

        public PostBuilder CommandRejection(CommandRejectionReason reason, IEnumerable<string>? items = null)
            => new PostBuilder(config, PostType.CommandRejection)
                .WithOptionalItems(items)
                .WithRejectionReason(reason);

        public PostBuilder Unspecified(string text)
            => new PostBuilder(config, PostType.Unspecified)
                .WithText(text);

    }
}

