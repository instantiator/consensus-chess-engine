using System;
namespace ConsensusChessShared.Exceptions
{
	public class MoveFormatException : Exception
	{
		public string MoveText { get; set; }

		public MoveFormatException(string moveText, string message, Exception? innerException = null)
			: base(message, innerException)
		{
			MoveText = moveText;
		}
	}
}

