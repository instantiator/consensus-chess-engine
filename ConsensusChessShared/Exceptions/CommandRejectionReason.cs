using System;
namespace ConsensusChessShared.Exceptions
{
	public enum CommandRejectionReason
	{
        UnrecognisedCommand,
        NotAuthorised,
        NoCommandWords,
        CommandMalformed,
        UnexpectedException
    }
}

