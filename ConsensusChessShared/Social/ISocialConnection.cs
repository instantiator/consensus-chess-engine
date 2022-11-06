using System;
namespace ConsensusChessShared.Social
{
	public interface ISocialConnection
	{
		Task<string> GetDisplayNameAsync();
		Task<IEnumerable<SocialCommand>> RetrieveComamndsAsync(DateTime since, DateTime until);
		void StartListening(Action<SocialCommand> action);
		void StopListening();
	}
}

