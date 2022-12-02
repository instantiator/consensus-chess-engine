using System;
using System.Text;
using System.Web;
using ConsensusChessShared.Migrations;

namespace ConsensusChessShared.Helpers
{
	public static class StringHelpers
	{
		public static string? Capitalise(this string? text)
			=> string.IsNullOrWhiteSpace(text)
				? text
				: text.Length > 1
					? $"{text[0].ToString().ToUpper()}{text.Substring(1)}"
					: text[0].ToString().ToUpper();

		public static string RestoreUnicode(this string text)
			=> HttpUtility.HtmlDecode(text);
	}

	
}

