using System;
using System.Runtime.Serialization;

namespace ConsensusChessShared.Exceptions
{
    public class CommandRejectionException : Exception
    {
        public CommandRejectionReason Reason { get; private set; }
        public IEnumerable<string> Words { get; private set; }
        public string SenderId { get; private set; }

        public CommandRejectionException(IEnumerable<string> words, string senderId, CommandRejectionReason reason, string? message = null) : base(message)
        {
            Reason = reason;
            Words = words;
            SenderId = senderId;
        }
    }
}

