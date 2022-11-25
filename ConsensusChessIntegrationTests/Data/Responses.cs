using System;
using ConsensusChessShared.Constants;

namespace ConsensusChessIntegrationTests.Data
{
	public class Responses
	{
		public static string NewGame_reply(SideRules rules, IEnumerable<string> shortcodes)
			=> $"New {rules.ToString()} game for: {string.Join(", ", shortcodes.Distinct())}";

		public static string NewGame_announcement(SideRules rules)
			=> $"New {rules.ToString()} game started.";

		public static string NewBoard()
			=> "New board.";

		public static string MoveAccepted()
			=> "Move accepted - thank you";
    }
}

