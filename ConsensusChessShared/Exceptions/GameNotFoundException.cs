using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;

namespace ConsensusChessShared.Exceptions
{
    public class GameNotFoundException : Exception
    {
        public SocialCommand Command { get; set; }
        public GameNotFoundReason Reason { get; set; }

        public GameNotFoundException(SocialCommand cmd, GameNotFoundReason reason) : base(reason.ToString())
        {
            this.Command = cmd;
            this.Reason = reason;
        }
    }
}

