using System;
namespace ConsensusChessShared.Constants
{
	public enum PostType
	{
		SocialStatus,

		CommandRejection,
		CommandResponse,

		Node_VoteAccepted,
		Node_VoteSuperceded,
		Node_GameNotFound,
		Node_MoveValidation,

		Engine_GameCreationResponse,
		Engine_GameAnnouncement,
		Engine_GameAdvance,
		Engine_GameAbandoned,
        Engine_GameEnded,

        Node_BoardUpdate,
		Node_BoardReminder,
		Node_VotingInstructions,
		Node_FollowInstructions,
		Node_GameAbandonedUpdate,
		Node_GameEndedUpdate,

		Unspecified,
	}

}

