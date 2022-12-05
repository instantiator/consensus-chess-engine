using System;
namespace ConsensusChessIntegrationTests.Data
{
	public class Messages
	{
		public static string NewGameCommand(string title, string description, IEnumerable<string> shortcodes)
			=> $"new \"{title}\" \"{description}\" {string.Join(" ", shortcodes)}";

		public static string Move(string from, string to)
			=> $"move {from} - {to}";

	}
}

