using System;
using ConsensusChessShared.Constants;

namespace ConsensusChessIntegrationTests.Data
{
	public class Snippets
	{
		public static string[] Engine_StatusParts()
			=> new[] { "inactive games:", "active games:", "nodes:" };

		public static string Engine_NewGame_reply(SideRules rules, IEnumerable<string> shortcodes)
			=> $"new {rules.ToString()} game for: {string.Join(", ", shortcodes.Distinct())}";

		public static string Engine_NewGame_announcement(SideRules rules)
			=> $"new {rules.ToString()} game started";

		public static string Node_NewBoard()
			=> "new board";

		public static string Node_MoveAccepted()
			=> "vote accepted";
    }
}

