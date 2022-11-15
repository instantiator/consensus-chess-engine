using System;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;

namespace ConsensusChessShared.Exceptions
{
    public class VoteRejectionException : Exception
    {
        public VoteValidationState Reason { get; set; }
        public SocialCommand? Command { get; set; }

        // TODO: better rejection content - ensure the message/reason are available as a human readable string
        public VoteRejectionException(VoteValidationState reason, SocialCommand? cmd = null, string? message = null, Exception? innerException = null)
            : base(message, innerException)
        {
            this.Command = cmd;
            this.Reason = reason;
        }
    }
}

