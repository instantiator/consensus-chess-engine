using System;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;

namespace ConsensusChessShared.Exceptions
{
    public class VoteRejectionException : Exception
    {
        public Vote Vote { get; set; }
        public VoteValidationState Reason { get; set; }
        public SocialCommand? Command { get; set; }
        public string? Detail { get; set; }

        // TODO: better rejection content - ensure the message/reason are available as a human readable string
        public VoteRejectionException(Vote vote, VoteValidationState reason, SocialCommand? cmd = null, string? detail = null, Exception? innerException = null)
            : base(reason.ToString(), innerException)
        {
            Command = cmd;
            Reason = reason;
            Detail = detail;
            Vote = vote;
        }
    }
}

