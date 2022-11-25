using System;
namespace ConsensusChessIntegrationTests.Data
{
	public class Messages
	{
		public static string NewGameCommand(IEnumerable<string> shortcodes)
			=> $"new {string.Join(" ", shortcodes)}";

		public static string Move(string from, string to)
			=> $"move {from} - {to}";

	}
}

