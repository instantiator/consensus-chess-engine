using System;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using Mastonet;
using Mastonet.Entities;

namespace ConsensusChessIntegrationTests
{
	public abstract class AbstractIntegrationTests
	{
        protected static readonly HttpClient http = new HttpClient();

        protected ConsensusChessDbContext GetDb()
            => ConsensusChessDbContext.FromEnvironment(Environment.GetEnvironmentVariables());

        protected Network GetNetwork()
            => Network.FromEnvironment(Environment.GetEnvironmentVariables());

        protected MastodonClient GetMastodonClient()
        {
            var network = GetNetwork();

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

            return new MastodonClient(reg, token, http);
        }

    }
}

