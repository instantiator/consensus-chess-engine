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

        public override string? DisplayName => user?.DisplayName;
        public override string? AccountName => user?.AccountName;

        public Dictionary<PostType, Visibility> VisibilityMapping = new Dictionary<PostType, Visibility>()
        {
            { PostType.CommandResponse, Visibility.Direct },
            { PostType.MoveValidation, Visibility.Direct },
            { PostType.GameAnnouncement, Visibility.Unlisted }, // TODO: public when live
            { PostType.BoardUpdate, Visibility.Unlisted },      // TODO: public when live
            { PostType.SocialStatus, Visibility.Private },
            { PostType.Unspecified, Visibility.Unlisted },
        };

        public override async Task<Post> PostToNetworkAsync(Post post, bool dryRun)
        {
            Visibility visibility = VisibilityMapping[post.Type];

            try
            {
                if (!dryRun)
                {
                    log.LogDebug($"Posting to network...");
                    var status = await client.PostStatus(post.Message, visibility: visibility, replyStatusId: post.NetworkReplyToId);
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

        protected override async Task MarkCommandProcessedAsync(long id)
        {
            log.LogDebug($"Favouriting status: {id}");
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
        /// All the words that should be skipped when processing a command.
        /// ie. the account name (short and long forms).
        /// NB. this is a bit defensive. I know - I just wanted it to work.
        /// </summary>
        public override IEnumerable<string> CalculateCommandSkips() => new[]
        {
            $"@{AccountName}",
            $"@{AccountName}@{network.NetworkServer}",
            $"{AccountName}",
            $"{AccountName}@{network.NetworkServer}",
        };

        public SocialCommand ConvertToSocialCommand(Notification notification, bool isRetrospective)
        {
            // TODO: isForMe not perfect...

            log.LogDebug($"CommandSkips: {string.Join(", ",CalculateCommandSkips())}");
            log.LogDebug($"Mentions: {string.Join(", ", notification.Status?.Mentions.Select(m => m.AccountName) ?? new string[] { })}");

            var isForMe = notification.Status?.Mentions.Any(m => CalculateCommandSkips().Contains(m.AccountName)) ?? false;
            var isFrom = notification.Status!.Account.AccountName;
            var isFavourited = notification.Status?.Favourited ?? false; // favourited == processed
            var isAuthorised = network.AuthorisedAccountsList.Contains(isFrom);

            return new SocialCommand()
            {
                Network = network,
                NetworkUserId = notification.Status!.Account.AccountName,
                RawText = notification.Status!.Content,
                SourceId = notification.Status!.Id,
                InReplyToId = notification.Status!.InReplyToId,
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
