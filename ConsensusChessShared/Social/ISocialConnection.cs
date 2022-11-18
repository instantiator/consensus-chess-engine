using System;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Social
{
	public interface ISocialConnection
	{
		Task InitAsync(NodeState state);
		string? DisplayName { get; }
		string? AccountName { get; }

        event Func<NodeState, Task> OnStateChange;

        IEnumerable<string> CalculateCommandSkips();

		Task StartListeningForCommandsAsync(Func<SocialCommand, Task> receiver, bool retrieveMissedCommands);
		Task StopListeningForCommandsAsync(Func<SocialCommand, Task> receiver);

        Task<Post> PostAsync(SocialStatus status, bool? dryRun = null);
		Task<Post> PostAsync(Game game, bool? dryRun = null);
        Task<Post> PostAsync(Game game, Board board, bool? dryRun = null);
        Task<Post> PostAsync(string text, PostType type, bool? dryRun = null);
		Task<Post> ReplyAsync(SocialCommand origin, string message, PostType? postType = null, bool? dryRun = null);
        Task<Post> PostToNetworkAsync(Post post, bool dryRun);
    }
}

