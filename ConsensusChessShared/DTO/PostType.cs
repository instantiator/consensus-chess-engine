using System;
namespace ConsensusChessShared.DTO
{
	public enum PostType
	{
		SocialStatus,
		GameCreationResponse,
		GameAnnouncement,
		BoardUpdate,
		MoveAccepted,
		GameNotFound,
		MoveValidation,
		Unspecified,
		CommandRejection,
		CommandResponse
	}

}

