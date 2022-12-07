using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using Mastonet.Entities;

namespace ConsensusChessShared.Social
{
	public interface ISocialConnection
	{
		bool Ready { get; }
		bool Streaming { get; }
        bool Paused { get; }

		Task InitAsync(NodeState state);
        SocialUsername? Username { get; }

		void PauseStream();
		void ResumeStream();

        event Func<NodeState, Task> OnStateChange;

        IEnumerable<string> CalculateCommandSkips();

        Task<IEnumerable<SocialCommand>> GetAllNotificationSinceAsync(bool isRetrospective, string? sinceId, DateTime? orSinceWhen = null);
        Task ProcessCommandAsync(SocialCommand command);

        Task StartListeningForCommandsAsync(Func<SocialCommand, Task> receiver, bool retrieveMissedCommands);
		Task StopListeningForCommandsAsync(Func<SocialCommand, Task> receiver);

        Task<Post> PostAsync(Post post, bool? dryRun = null);
    }
}

