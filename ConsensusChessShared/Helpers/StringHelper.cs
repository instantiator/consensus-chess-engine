using System;
using ConsensusChessShared.Migrations;

namespace ConsensusChessShared.Helpers
{
	public static class StringHelpers
	{
		public static string? Capitalise(this string? text)
		=> text == null
			? null
			: text == ""
				? ""
				: text.Length > 1
					? $"{text[0].ToString().ToUpper()}{text.Substring(1)}"
					: text[0].ToString().ToUpper();
	}
}

