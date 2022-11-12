using System;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using Mastonet;
using Mastonet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            // delete sent messages
            foreach (var status in SentMessages)
                await social.DeleteStatus(status.Id);

            // crucially, don't delete node_status from the db
            var tables = new[]
            {
                "board","commitment","games","media","move","participant","post","vote","vote_validation"
            };

            using (var db = GetDb())
            {
                foreach (var table in tables)
                {
                    // this is postgres SQL, see: https://www.postgresql.org/docs/current/sql-truncate.html
                    await db.Database.ExecuteSqlRawAsync($"TRUNCATE {table} CASCADE;");
                }
            }
        }

        protected async Task DeleteAllDataAsync()
        {
            using (var db = GetDb())
            {
                var tables = db.Model.GetEntityTypes()
                    .SelectMany(t => t.GetTableMappings())
                    .Select(m => m.Table.Name)
                    .Distinct()
                    .ToList();

                foreach (var table in tables)
                {
                    // this is postgres SQL, see: https://www.postgresql.org/docs/current/sql-truncate.html
                    await db.Database.ExecuteSqlRawAsync($"TRUNCATE {table} CASCADE;");
                }
            }

        }

    }
}
