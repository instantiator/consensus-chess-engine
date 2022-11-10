using System;
using ConsensusChessShared.DTO;

namespace ConsensusChessShared.Social
{
	public interface ISocialConnection
	{
		Task InitAsync();
		string DisplayName { get; }
		string AccountName { get; }

        event Func<NodeState, Task> OnStateChange;

        IEnumerable<string> CalculateCommandSkips();

		Task StartListeningForCommandsAsync(Func<SocialCommand, Task> receiver, bool retrieveMissedCommands);
		Task StopListeningForCommandsAsync(Func<SocialCommand, Task> receiver);

        Task<PostReport> PostAsync(SocialStatus status);
		Task<PostReport> PostAsync(Game game);
        Task<PostReport> PostAsync(Game game, Board board);
        Task<PostReport> PostAsync(string text, PostType type);
		Task<PostReport> ReplyAsync(SocialCommand origin, string message);
        Task<PostReport> PostToNetworkAsync(Post post);

    }
}

