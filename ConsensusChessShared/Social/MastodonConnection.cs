using System;
using ConsensusChessShared.DTO;
using Mastonet;
using Mastonet.Entities;

namespace ConsensusChessShared.Social
{
	public class MastodonConnection : ISocialConnection
	{
        private static readonly HttpClient http = new HttpClient();

        private Network network;
        private Action<SocialCommand>? receiver;
        private MastodonClient? client;

		public MastodonConnection(Network network)
		{
			this.network = network;

            // https://github.com/glacasa/Mastonet/blob/main/DOC.md
            // https://github.com/glacasa/Mastonet/blob/main/API.md

            AppRegistration reg = new AppRegistration()
            {
                ClientId = network.AppKey,
                ClientSecret = network.AppSecret,
                Instance = network.NetworkServer,
                Scope = Scope.Read | Scope.Write
            };

            Auth token = new Auth()
            {
                AccessToken = network.AppToken
            };

            client = new MastodonClient(reg, token, http);
        }

        public async Task<string> GetDisplayNameAsync()
        {
            var user = await client.GetCurrentUser();
            return user.DisplayName;
        }

        public async Task<IEnumerable<SocialCommand>> RetrieveComamndsAsync(DateTime since, DateTime until)
        {
            throw new NotImplementedException();
            // TODO
        }

        public void StartListening(Action<SocialCommand> action)
        {
            this.receiver = action;
            // TODO
        }

        public void StopListening()
        {
            throw new NotImplementedException();
            // TODO
        }
    }
}

