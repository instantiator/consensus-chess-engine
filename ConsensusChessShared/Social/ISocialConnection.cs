using System;
namespace ConsensusChessShared.Social
{
	public interface ISocialConnection
	{
		Task<string> GetDisplayNameAsync();
		void StartListening(Action<SocialCommand> receiver, DateTime? backdate);
		void StopListening(Action<SocialCommand> receiver);

        Task<PostReport> PostStatusAsync(SocialStatus status);
        Task<PostReport> PostStatusAsync(string detail);
	}
}

