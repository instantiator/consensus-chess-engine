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
        private Account? user;

        private TimelineStreaming? stream;

        // TODO: revisit the paging limit - may or may not be necessary
        private const int MAX_PAGES = 100;

        public MastodonConnection(ILogger log, Network network, NodeState state, bool dryRuns) : base(log, network, state, dryRuns)
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

            client = new MastodonClient(reg, token, http);
        }

        public override async Task InitAsync()
        {
            user = await client.GetCurrentUser();
        }

        public override string DisplayName => user!.DisplayName;
        public override string AccountName => user!.AccountName;

        public override async Task<Post> PostToNetworkAsync(Post post, bool dryRun)
        {
            try
            {
                if (!dryRun)
                {
                    log.LogDebug($"Posting to network...");
                    var status = await client.PostStatus(post.Message, replyStatusId: post.ReplyTo);
                }
                else
                {
                    log.LogWarning($"Dry run.");
                }
                post.Succeed();
            }
            catch (Exception e)
            {
                post.Fail(e: e);
            }
            return post;
        }

        protected override async Task StartListeningForNotificationsAsync()
        {
            // set up the stream
            stream = client.GetUserStreaming();
            stream.OnNotification += Stream_OnNotification;
            log.LogDebug("Starting stream");
            stream.Start(); // not awaited - awaiting blocks
        }

        protected override async Task GetMissedCommands()
        {
            // fetch conversations up to now before starting to stream
            var firstStart = state.LastNotificationId == 0;

            log.LogDebug(
                !firstStart
                    ? $"Retrieving any missed notifications since: {state.LastNotificationId}"
                    : $"Skipping missed commands - this is a first start.");

            if (!firstStart)
            {
                var missedNotifications = await GetAllNotificationSinceAsync(state.LastNotificationId);
                missedCommands = missedNotifications.Select(n => ConvertToSocialCommand(n, true));
            }
            else
            {
                missedCommands = null;
            }
        }

        protected override async Task<IEnumerable<Notification>> GetAllNotificationSinceAsync(long sinceId)
        {
            var list = new List<Notification>();
            long? nextPageMaxId = null;
            int iterations = 0;

            do
            {
                var page = await client.GetNotifications(sinceId: sinceId, maxId: nextPageMaxId);
                list.AddRange(page.Where(pn => !list.Select(n => n.Id).Contains(pn.Id)));
                nextPageMaxId = page.NextPageMaxId;
                iterations++;
            } while (nextPageMaxId != null && iterations < MAX_PAGES);

            return list;
        }

        private async void Stream_OnNotification(object? sender, StreamNotificationEventArgs e)
        {
            var cmd = ConvertToSocialCommand(e.Notification, false);
            if (cmd != null)
            {
                await ProcessCommand(cmd);
            }
        }

        protected override async Task MarkCommandProcessed(long id)
        {
            await client.Favourite(id);
        }

        public override async Task StopListeningForCommandsAsync(Func<SocialCommand, Task> asyncCommandReceiver)
        {
            log.LogDebug("Removing command listeners");
            if (stream != null)
            {
                stream.OnNotification -= Stream_OnNotification;
                stream.Stop();
                stream = null;
            }

            asyncCommandReceivers -= asyncCommandReceiver;
        }

        /// <summary>
        /// All the words that should be skipped when processing a command
        /// ie. the account name (short and long forms)
        /// </summary>
        public override IEnumerable<string> CalculateCommandSkips() => new[]
        {
            $"@{AccountName}",
            $"@{AccountName}@{network.NetworkServer}",
        };

        public SocialCommand ConvertToSocialCommand(Notification notification, bool isRetrospective)
        {
            var isForMe = notification.Status?.Mentions.Any(m => m.AccountName == AccountName) ?? false;
            var isFrom = notification.Status?.Account.AccountName;
            var isFavourited = notification.Status?.Favourited ?? false; // favourited == processed
            var isAuthorised = network.AuthorisedAccountsList.Contains(isFrom);

            return new SocialCommand()
            {
                Network = network,
                NetworkUserId = notification.Status!.Account.AccountName,
                RawText = notification.Status!.Content,
                SourceId = notification.Status!.Id,
                SourceAccount = isFrom,
                IsAuthorised = isAuthorised,
                IsRetrospective = isRetrospective,
                IsForThisNode = isForMe,
                IsProcessed = isFavourited,
                DeliveryMedium = "mastodon",
                DeliveryType = notification.Type
            };

        }
    }
}
