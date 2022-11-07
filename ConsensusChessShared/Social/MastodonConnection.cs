using System;
using ConsensusChessShared.DTO;
using Mastonet;
using Mastonet.Entities;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public class MastodonConnection : AbstractSocialConnection
	{
        private static readonly HttpClient http = new HttpClient();

        private MastodonClient client;
        private event Action<SocialCommand> receivers;

		public MastodonConnection(ILogger log, Network network) : base(log, network)
		{
			

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

        public override async Task<string> GetDisplayNameAsync()
        {
            var user = await client.GetCurrentUser();
            return user.DisplayName;
        }

        protected override async Task<PostReport> PostToNetworkAsync(string text)
        {
            try
            {
                var status = await client.PostStatus(text);
                return PostReport.Success();
            }
            catch (Exception e)
            {
                return PostReport.From(e);
            }
        }

        public override void StartListening(Action<SocialCommand> receiver, DateTime? backdate)
        {
            this.receivers += receiver;
            // TODO
        }

        public override void StopListening(Action<SocialCommand> receiver)
        {
            this.receivers -= receiver;
            // TODO
        }
    }
}

