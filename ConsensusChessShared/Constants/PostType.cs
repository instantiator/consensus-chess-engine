﻿using System;
namespace ConsensusChessShared.Constants
{
	public enum PostType
	{
		SocialStatus,

		CommandRejection,
		CommandResponse,

		MoveAccepted,
		GameNotFound,
		MoveValidation,

		Engine_GameCreationResponse,
		Engine_GameAnnouncement,
		Engine_GameAdvance,
		Engine_GameAbandoned,

		Node_BoardUpdate,

		Unspecified,
	}

}
