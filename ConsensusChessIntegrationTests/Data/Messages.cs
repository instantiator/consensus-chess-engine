using System;
using ConsensusChessShared.Constants;

namespace ConsensusChessIntegrationTests.Data
{
	public class Messages
	{
		public static string NewGameCommand(SideRules rules, string title, string description, IEnumerable<string> shortcodes)
			=> $"new {rules} \"{title}\" \"{description}\" {string.Join(" ", shortcodes)}";

		public static string Move(string from, string to)
			=> $"move {from} - {to}";

	}
}

