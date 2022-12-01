using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Social;
using static ConsensusChessShared.Content.BoardFormatter;
using static ConsensusChessShared.Content.BoardGraphicsData;

namespace ConsensusChessShared.Content
{
	public partial class PostBuilder
	{
        public static PostBuilder Create(PostType type)
            => new PostBuilder(type);

        public static PostBuilder SocialStatus(NodeState state, SocialStatus status)
            => new PostBuilder(PostType.SocialStatus)
                .WithNodeState(state)
                .WithSocialStatus(status);

        public static PostBuilder CommandResponse(string text)
            => new PostBuilder(PostType.CommandResponse)
                .WithText(text);

        public static PostBuilder Engine_GameCreationResponse(Game game)
            => new PostBuilder(PostType.Engine_GameCreationResponse)
                .WithGame(game);

        public static PostBuilder Engine_GameAnnouncement(Game game)
            => new PostBuilder(PostType.Engine_GameAnnouncement)
                .WithGame(game);

        public static PostBuilder Engine_GameAbandoned(Game game)
            => new PostBuilder(PostType.Engine_GameAbandoned)
                .WithGame(game);

        public static PostBuilder Engine_GameEnded(Game game)
            => new PostBuilder(PostType.Engine_GameEnded)
                .WithGame(game);

        public static PostBuilder Engine_GameAdvance(Game game)
            => new PostBuilder(PostType.Engine_GameAdvance)
                .WithGame(game);

        public static PostBuilder Node_BoardUpdate(Game game, Board board, BoardFormat format, BoardStyle style)
            => new PostBuilder(PostType.Node_BoardUpdate)
                .WithGame(game)
                .WithBoard(board, format)
                .AndBoardGraphic(style, format);

        public static PostBuilder Node_GameAbandonedUpdate(Game game)
            => new PostBuilder(PostType.Node_GameAbandonedUpdate)
                .WithGame(game);

        public static PostBuilder Node_GameEndedUpdate(Game game)
            => new PostBuilder(PostType.Node_GameEndedUpdate)
                .WithGame(game);

        public static PostBuilder GameNotFound(GameNotFoundReason reason)
            => new PostBuilder(PostType.GameNotFound)
                .WithGameNotFoundReason(reason);

        public static PostBuilder CommandRejection(CommandRejectionReason reason, IEnumerable<string>? items = null)
            => new PostBuilder(PostType.CommandRejection)
                .WithOptionalItems(items)
                .WithRejectionReason(reason);

        public static PostBuilder MoveAccepted(Game game, Side side, Vote vote)
            => new PostBuilder(PostType.MoveAccepted)
                .WithGame(game)
                .WithSide(side)
                .WithVote(vote);

        public static PostBuilder MoveValidation(VoteValidationState state, SocialUsername sender, string movetext, string? detail)
            => new PostBuilder(PostType.MoveValidation)
                .WithValidationState(state)
                .WithUsername(sender)
                .WithMoveText(movetext)
                .WithDetail(detail);

        public static PostBuilder Unspecified(string text)
            => new PostBuilder(PostType.Unspecified)
                .WithText(text);

    }
}

