using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Social
{
	public interface ISocialConnection
	{
		bool Ready { get; }
		Task InitAsync(NodeState state);
        SocialUsername? Username { get; }

        event Func<NodeState, Task> OnStateChange;

        IEnumerable<string> CalculateCommandSkips();

		Task StartListeningForCommandsAsync(Func<SocialCommand, Task> receiver, bool retrieveMissedCommands);
		Task StopListeningForCommandsAsync(Func<SocialCommand, Task> receiver);

        Task<Post> PostAsync(Post post, bool? dryRun = null);
    }
}

