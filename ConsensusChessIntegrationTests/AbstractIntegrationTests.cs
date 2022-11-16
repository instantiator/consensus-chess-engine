using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Helpers;
using ConsensusChessShared.Social;
using Mastonet;
using Mastonet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace ConsensusChessIntegrationTests
{
	public abstract class AbstractIntegrationTests
	{
        protected string logPath = "/logs/integration-tests.log";

        public TestContext TestContext { get; set; }

        public const int TIMEOUT_mins = 10;

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
            message = string.IsNullOrWhiteSpace(directRecipient) ? message : $"@{directRecipient} {message}";

            WriteLogLine($"Sending message: {message}\nVisibility: {visibility}, InReplyTo: {inReplyTo?.ToString() ?? "(none)"}");

            var status = await social.PostStatus(message, visibility: visibility, replyStatusId: inReplyTo);
            SentMessages.Add(status);
            return status;
        }

        protected async Task<IEnumerable<Notification>> AwaitNotificationsAsync(TimeSpan timeoutAfter, Func<Notification,bool> matcher, int expect = 1)
        {
            WriteLogLine("Awaiting notifications...");

            var timeout = DateTime.Now.Add(timeoutAfter);
            List<Notification> notifications = new List<Notification>();
            do
            {
                var matched = ReceivedNotifications.Where(n => matcher(n)).ToList();
                notifications.AddRange(matched);

                if (matched.Count() > 0) { WriteLogLine($"Matched {matched.Count()} notifications of: {expect}"); }

                if (notifications.Count() < expect)
                    await Task.Delay(TimeSpan.FromSeconds(15));
            }
            while (notifications.Count() < expect && DateTime.Now < timeout);
            return notifications;
        }

        protected async Task<IEnumerable<Status>> PollForStatusesAsync(TimeSpan timeoutAfter, long accountId, Func<Status, string, bool> matcher, int expect = 1)
        {
            WriteLogLine("Polling for statuses...");

            var timeout = DateTime.Now.Add(timeoutAfter);
            List<Status> statuses = new List<Status>();
            do
            {
                var found = await social.GetAccountStatuses(accountId);
                var newFound = found.Where(nf => !statuses.Select(s => s.Id).Contains(nf.Id));
                if (newFound.Count() > 0) { WriteLogLine($"Found:\n{string.Join("\n", "  * " + newFound.Select(nf => nf.Content))}"); }
                var matched = newFound.Where(s => matcher(s, CommandHelper.CleanupStatus(s.Content)));
                statuses.AddRange(matched);

                if (matched.Count() > 0) { Console.WriteLine($"Matched {matched.Count()} statuses of: {expect}"); }

                if (statuses.Count() < expect)
                    await Task.Delay(TimeSpan.FromSeconds(15));
            }
            while (statuses.Count() < expect && DateTime.Now < timeout);
            return statuses;
        }

        [TestInitialize]
        public void TestInit()
        {
            WriteLogHeader($"{TestContext.TestName}");

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
            WriteLogLine($"Stopping {TestContext.TestName}");

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
                    var sql = $"TRUNCATE {table} CASCADE;";
                    WriteLogLine(sql);
                    // this is postgres SQL, see: https://www.postgresql.org/docs/current/sql-truncate.html
                    await db.Database.ExecuteSqlRawAsync(sql);
                }
            }

            // finished - leave a space
            WriteLogLine();
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

        protected async Task AssertFavouritedAsync(Status status)
        {
            WriteLogLine("Waiting for status to be favourited...");
            Assert.IsNotNull(status);
            var notification_favourite = await AwaitNotificationsAsync(
                TimeSpan.FromMinutes(TIMEOUT_mins),
                (n) => n.Type == "favourite" && n.Status != null && n.Status.Id == status.Id);

            Assert.IsNotNull(notification_favourite.Single().Status);
            Assert.AreEqual(notification_favourite.Single().Status!.Id, status.Id);
        }

        protected async Task<Notification> AssertGetsReplyNotificationAsync(Status status, string expectedReply)
        {
            WriteLogLine("Waiting for reply to status...");
            var replyNotifications = await AssertAndGetReplyNotificationsAsync(status, 1);
            var replyContent = CommandHelper.RemoveUnwantedTags(replyNotifications.Single().Status!.Content);
            Assert.AreEqual(expectedReply, replyContent);
            return replyNotifications.Single();
        }

        protected async Task<IEnumerable<Notification>> AssertAndGetReplyNotificationsAsync(Status status, int expected)
        {
            var notifications_replyNewGame = await AwaitNotificationsAsync(
                TimeSpan.FromMinutes(TIMEOUT_mins),
                (n) => n.Type == "mention" && n.Status != null && n.Status.InReplyToId == status.Id, expected);

            Assert.IsNotNull(notifications_replyNewGame);
            Assert.AreEqual(expected, notifications_replyNewGame.Count());
            return notifications_replyNewGame;
        }

        protected async Task<IEnumerable<Status>> AssertAndGetStatusesAsync(Account account, int expected, Func<Status, string, bool> matcher)
        {
            var statuses = await PollForStatusesAsync(
                TimeSpan.FromMinutes(TIMEOUT_mins),
                account.Id,
                matcher,
                expected);
            Assert.AreEqual(expected, statuses.Count());
            return statuses;
        }

        protected async Task<Account> GetAccountAsync(string fullHandle)
        {
            var accountCandidates = await social.SearchAccounts(fullHandle);
            Assert.AreEqual(1, accountCandidates.Count());
            Assert.AreEqual(fullHandle, accountCandidates.Single().AccountName);
            return accountCandidates.Single();
        }

        protected void WriteLogHeader(string header)
        {
            File.AppendAllLines(logPath, new[] { "## " + header, "" });
        }

        protected void WriteLogLine(string log)
        {
            File.AppendAllLines(logPath, new[] { "* " + log });
        }

        protected void WriteLogLine()
        {
            File.AppendAllLines(logPath, new[] { "" });
        }
    }
}
