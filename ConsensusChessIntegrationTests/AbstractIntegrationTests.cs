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

        protected List<Status> SentMessages { get; } = new List<Status>();

        protected ConsensusChessDbContext GetDb()
            => ConsensusChessDbContext.FromEnvironment(Environment.GetEnvironmentVariables());

        protected Network GetNetwork()
            => Network.FromEnvironment(Environment.GetEnvironmentVariables());

        protected MastodonClient social;

        protected AbstractIntegrationTests()
        {
            social = GetMastodonClient();
        }

        private MastodonClient GetMastodonClient()
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

        protected async Task<Status> SendMessageAsync(string message, Visibility? visibilityOverride = null, string? directRecipient = null, long? inReplyTo = null)
        {
            var visibility =
                visibilityOverride != null
                ? visibilityOverride
                : Visibility.Direct;
            message = string.IsNullOrWhiteSpace(directRecipient) ? message : $"{directRecipient} {message}";
            var status = await social.PostStatus(message, visibility: visibility, replyStatusId: inReplyTo);
            SentMessages.Add(status);
            return status;
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            foreach (var status in SentMessages)
                await social.DeleteStatus(status.Id);
        }

    }
}

