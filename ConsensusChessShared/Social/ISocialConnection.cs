using System;
namespace ConsensusChessShared.Social
{
	public interface ISocialConnection
	{
		Task InitAsync();
		string DisplayName { get; }
		string AccountName { get; }

		Task StartListeningForCommandsAsync(Func<SocialCommand, Task> receiver, long? sinceId);
		Task StopListeningForCommandsAsync(Func<SocialCommand, Task> receiver);

        Task<PostReport> PostAsync(SocialStatus status);
		Task<PostReport> PostAsync(string text);

    }
}

