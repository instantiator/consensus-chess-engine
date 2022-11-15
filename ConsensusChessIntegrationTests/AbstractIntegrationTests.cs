using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using Mastonet;
using Mastonet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace ConsensusChessIntegrationTests
{
	public abstract class AbstractIntegrationTests
	{
        protected static readonly HttpClient http = new HttpClient();

        protected List<Status> SentMessages { get; } = new List<Status>();

        protected ConcurrentBag<Notification> ReceivedNotifications { get; } = new ConcurrentBag<Notification>();

        protected Dictionary<string, string> accounts;

        protected ConsensusChessDbContext GetDb()
            => ConsensusChessDbContext.FromEnvironment(Environment.GetEnvironmentVariables());

        protected Network GetNetwork()
            => Network.FromEnvironment(Environment.GetEnvironmentVariables());

        protected MastodonClient social;
        protected TimelineStreaming? stream;

        protected AbstractIntegrationTests()
        {
            social = GetMastodonClient();
            var environment = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);

            accounts = new Dictionary<string, string>()
            {
                { "engine", environment["INT_ENGINE_ACCOUNT"] },
                { "node", environment["INT_NODE_ACCOUNT"] },
            };
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

        protected async Task<IEnumerable<Notification>> AwaitNotifications(TimeSpan timeoutAfter, Func<Notification,bool> matcher, int expect = 1)
        {
            var timeout = DateTime.Now.Add(timeoutAfter);
            List<Notification> notifications = new List<Notification>();
            do
            {
                var matched = ReceivedNotifications.Where(n => matcher(n)).ToList();
                notifications.AddRange(matched);

                if (matched.Count() > 0) { Console.WriteLine($"Matched {matched.Count()} notifications. Expected: {expect}"); }

                if (ReceivedNotifications.Count() < expect)
                    await Task.Delay(TimeSpan.FromSeconds(10));
            }
            while (notifications.Count() < expect && DateTime.Now < timeout);
            return notifications;
        }

        [TestInitialize]
        public void TestInit()
        {
            stream = social.GetUserStreaming();
            stream.OnNotification += (obj,e) =>
            {
                ReceivedNotifications.Add(e.Notification);

            };
            stream.Start(); // not awaited - awaiting blocks
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            // stop listening
            stream!.Stop();
            ReceivedNotifications.Clear();

            // delete sent messages
            foreach (var status in SentMessages)
            {
                // await social.DeleteStatus(status.Id);
            }
            SentMessages.Clear();

            // crucially, don't delete node_status from the db
            var tables = new[]
            {
                "board","commitment","games","media","move","participant","post","vote"
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

        [Obsolete("No longer used, as we'd prefer the db to retain data about the node registrations")]
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
