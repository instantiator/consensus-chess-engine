using System;
using System.Runtime.Serialization;
using ConsensusChessShared.Social;

namespace ConsensusChessShared.Exceptions
{
    public class CommandRejectionException : Exception
    {
        public CommandRejectionReason Reason { get; private set; }
        public SocialCommand Command { get; private set; }
        public IEnumerable<string> Words { get; private set; }

        public CommandRejectionException(SocialCommand command, IEnumerable<string> parsedWords, CommandRejectionReason reason, string? message = null) : base(message)
        {
            Reason = reason;
            Command = command;
            Words = parsedWords;
        }
    }
}

