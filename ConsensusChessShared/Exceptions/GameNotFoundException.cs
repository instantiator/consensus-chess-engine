using System;
using ConsensusChessShared.Social;

namespace ConsensusChessShared.Exceptions
{
    public class GameNotFoundException : Exception
    {
        public SocialCommand Command { get; set; }

        public GameNotFoundException(SocialCommand cmd, string? message) : base(message)
        {
            this.Command = cmd;
        }
    }
}

