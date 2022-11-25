using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Helpers;
using ConsensusChessShared.Social;
using Mastonet;
using Mastonet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace ConsensusChessIntegrationTests
{
	public abstract class AbstractIntegrationTests
	{
        protected Mock<ILogger> mockLogger;

        protected string logPath;

        public TestContext? TestContext { get; set; }

        public const int TIMEOUT_mins = 15;

        protected static readonly HttpClient http = new HttpClient();

        protected List<Status> SentMessages { get; } = new List<Status>();

        protected ConcurrentBag<Notification> ReceivedNotifications { get; } = new ConcurrentBag<Notification>();

        protected Dictionary<NodeType, SocialUsername> contacts;

        protected ConsensusChessDbContext GetDb()
            => ConsensusChessPostgresContext.FromEnv(Environment.GetEnvironmentVariables());

        protected Network GetNetwork()
            => Network.FromEnv(Environment.GetEnvironmentVariables());

        protected MastodonClient social;
        protected Account? user;
        protected SocialUsername username;
        protected TimelineStreaming? stream;
        protected Network network;

        protected AbstractIntegrationTests()
        {
            if (Directory.Exists("/logs"))
            {
                logPath = "/logs/integration-tests.log";
            }
            else
            {
                var folder = Environment.SpecialFolder.LocalApplicationData;
                var path = Environment.GetFolderPath(folder);
                logPath = Path.Join(path, "integration-tests.log");
            }

            mockLogger = new Mock<ILogger>();
            network = GetNetwork();

            social = GetMastodonClient();
            var environment = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);

            var engineSocialUsername = SocialUsername.From(
                environment["INT_ENGINE_ACCOUNT"],
                "integration engine",
                network,
                environment["INT_ENGINE_SHORTCODE"]);

            var nodeSocialUsername = SocialUsername.From(
                environment["INT_NODE_ACCOUNT"],
                "integration node",
                network,
                environment["INT_NODE_SHORTCODE"]);

            contacts = new Dictionary<NodeType, SocialUsername>()
            {
                { NodeType.Engine, engineSocialUsername },
                { NodeType.Node, nodeSocialUsername },
            };
        }

        private MastodonClient GetMastodonClient()
        {
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

        protected async Task<Status> SendMessageAsync(string message, Visibility? visibilityOverride = null, SocialUsername? directRecipient = null, long? inReplyTo = null)
        {
            var visibility =
                visibilityOverride != null
                ? visibilityOverride
                : Visibility.Direct;
            message = directRecipient == null ? message : $"{directRecipient.AtFull} {message}";

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
                try
                {
                    var found = await social.GetAccountStatuses(accountId);
                    var newFound = found.Where(nf => !statuses.Select(s => s.Id).Contains(nf.Id));
                    if (newFound.Count() > 0) { WriteLogLine($"Found:\n{string.Join("\n", "  * " + newFound.Select(nf => nf.Content))}"); }
                    var matched = newFound.Where(s => matcher(s, CommandHelper.CleanupStatus(s.Content)));
                    statuses.AddRange(matched);
                    if (matched.Count() > 0) { WriteLogLine($"Matched {matched.Count()} statuses of: {expect}"); }
                }
                catch (Exception e)
                {
                    WriteLogLine($"{e.GetType().Name}: {e.Message}\n{e.StackTrace}");
                }

                if (statuses.Count() < expect)
                    await Task.Delay(TimeSpan.FromSeconds(15));
            }
            while (statuses.Count() < expect && DateTime.Now < timeout);
            return statuses;
        }

        [TestInitialize]
        public async Task TestInit()
        {
            WriteLogHeader($"{TestContext!.TestName}");

            user = await social.GetCurrentUser();
            username = SocialUsername.From(user.AccountName, "integration tester", network, "integration-tester");

            // crucially, don't delete node_status from the db
            var tables = new[]
            {
                "board","commitment","games","media","move","participant","post","vote"
            };

            using (var db = GetDb())
            {
                // ensure migrations are applied - just in case
                await db.Database.MigrateAsync();

                // now clear down tables
                foreach (var table in tables)
                {
                    var sql = $"TRUNCATE {table} CASCADE;";
                    WriteLogLine(sql);
                    // this is postgres SQL, see: https://www.postgresql.org/docs/current/sql-truncate.html
                    await db.Database.ExecuteSqlRawAsync(sql);
                }
            }

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

            WriteLogLine();
            return;
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

        protected async Task<Notification> AssertGetsReplyNotificationAsync(Status status, string expectedContains)
        {
            WriteLogLine($"Waiting for reply to status containing: {expectedContains}");
            var replyNotifications = await AssertAndGetReplyNotificationsAsync(status, 1);
            var replyContent = CommandHelper.RemoveUnwantedTags(replyNotifications.Single().Status!.Content);
            Assert.IsTrue(replyContent.Contains(expectedContains));
            return replyNotifications.Single();
        }

        protected async Task<IEnumerable<Notification>> AssertAndGetReplyNotificationsAsync(Status status, int expected)
        {
            var notifications_reply = await AwaitNotificationsAsync(
                TimeSpan.FromMinutes(TIMEOUT_mins),
                (n) => n.Type == "mention" && n.Status != null && n.Status.InReplyToId == status.Id, expected);

            Assert.IsNotNull(notifications_reply);
            Assert.AreEqual(expected, notifications_reply.Count());
            return notifications_reply;
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

        protected async Task<Account> GetAccountAsync(SocialUsername whom)
        {
            var accountCandidates = await social.SearchAccounts(whom.Full);
            Assert.AreEqual(1, accountCandidates.Count());
            Assert.IsTrue(whom.Equals(accountCandidates.Single().AccountName));
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
