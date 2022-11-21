using System;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Social
{
	public interface ISocialConnection
	{
		bool Ready { get; }
		Task InitAsync(NodeState state);
		string? DisplayName { get; }
		string? AccountName { get; }

        event Func<NodeState, Task> OnStateChange;

        IEnumerable<string> CalculateCommandSkips();

		Task StartListeningForCommandsAsync(Func<SocialCommand, Task> receiver, bool retrieveMissedCommands);
		Task StopListeningForCommandsAsync(Func<SocialCommand, Task> receiver);

		Task<Post> PostAsync(string text, PostType type = PostType.Unspecified, bool? dryRun = null);
        Task<Post> PostAsync(Post post, bool? dryRun = null);
    }
}

