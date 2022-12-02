using System;
using ConsensusChessShared.Constants;

namespace ConsensusChessIntegrationTests.Data
{
	public class Responses
	{
		public static string NewGame_reply(SideRules rules, IEnumerable<string> shortcodes)
			=> $"new {rules.ToString()} game for: {string.Join(", ", shortcodes.Distinct())}";

		public static string NewGame_announcement(SideRules rules)
			=> $"new {rules.ToString()} game started.";

		public static string NewBoard()
			=> "new board";

		public static string MoveAccepted()
			=> "move accepted";
    }
}

