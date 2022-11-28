using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
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

        public static int RATE_LIMIT_PERMITTED_REQUESTS = 300;
        public static TimeSpan RATE_LIMIT_PERMITTED_PERIOD = TimeSpan.FromSeconds(300);

        public MastodonConnection(ILogger log, Network network, string shortcode)
            : base(log, network, shortcode, RATE_LIMIT_PERMITTED_REQUESTS, RATE_LIMIT_PERMITTED_PERIOD)
		{
            // TODO: AppRegistration and Auth are no longer needed by Mastonet
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

            client = new MastodonClient(reg.Instance, token.AccessToken, http);
        }

        protected override async Task InitImplementationAsync()
        {
            await RateLimit();
            user = await client.GetCurrentUser();
            Username = SocialUsername.From(user.AccountName!, user.DisplayName!, network, shortcode);
        }

        public override SocialUsername? Username { get; set; }

        public static Dictionary<PostType, Visibility> VisibilityMapping = new Dictionary<PostType, Visibility>()
        {
            { PostType.SocialStatus, Visibility.Private },

            { PostType.CommandResponse, Visibility.Direct },
            { PostType.CommandRejection, Visibility.Direct },

            { PostType.MoveAccepted, Visibility.Direct },
            { PostType.MoveValidation, Visibility.Direct },
            { PostType.GameNotFound, Visibility.Direct },

            { PostType.Engine_GameAnnouncement, Visibility.Unlisted },
            { PostType.Engine_GameAdvance, Visibility.Unlisted },
            { PostType.Engine_GameCreationResponse, Visibility.Unlisted },
            { PostType.Engine_GameAbandoned, Visibility.Unlisted },
            { PostType.Engine_GameEnded, Visibility.Unlisted },

            { PostType.Node_BoardUpdate, Visibility.Unlisted }, // TODO: public when live
            { PostType.Node_GameAbandonedUpdate, Visibility.Unlisted }, // TODO: public when live
            { PostType.Node_GameEndedUpdate, Visibility.Unlisted }, // TODO: public when live

            { PostType.Unspecified, Visibility.Unlisted },
        };

        protected override async Task<Post> PostImplementationAsync(Post post, bool? dryRun)
        {
            Visibility visibility = VisibilityMapping[post.Type];
            post.AppName = network.AppName;
            post.NetworkServer = network.NetworkServer;
            post.NodeShortcode = state.Shortcode;
            var dry = dryRun ?? dryRuns;
            try
            {
                if (!dry)
                {
                    log.LogDebug($"Posting to network...");
                    // already rate limited through the public methods
                    var status = await client.PublishStatus(
                        status: post.Message,
                        visibility: visibility,
                        replyStatusId: post.NetworkReplyToId?.ToString());

                    post.Succeed(long.Parse(status.Id));
                }
                else
                {
                    log.LogWarning($"Dry run.");
                    post.Succeed(null);
                }
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
            await RateLimit();
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
                missedCommands = missedNotifications.Select(n => ConvertToSocialCommand(n, true)).OfType<SocialCommand>();
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
                ArrayOptions opts = new ArrayOptions()
                {
                    SinceId = sinceId.ToString(),
                    MaxId = nextPageMaxId.ToString()
                };

                await RateLimit();
                var page = await client.GetNotifications(options: opts);

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
            await RateLimit();
            await client.Favourite(id.ToString());
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
        /// TODO: this is a bit defensive. I know - I just wanted it to work.
        /// </summary>
        public override IEnumerable<string> CalculateCommandSkips() => new[]
        {
            Username!.Full,
            Username!.AtFull,
            Username!.Username,
            Username!.AtUsername,
        };

        public SocialCommand? ConvertToSocialCommand(Notification notification, bool isRetrospective)
        {
            // return null if the notification is not really a status
            if (notification.Type != "mention") { return null; }

            log.LogDebug($"Converting notification ({notification.Type}) to social command");
            log.LogDebug($"CommandSkips: {string.Join(", ",CalculateCommandSkips())}");
            log.LogDebug($"Mentions: {string.Join(", ", notification.Status?.Mentions.Select(m => m.AccountName) ?? new string[] { })}");

            // TODO: isForMe not perfect - better to be more specific about command skips
            var isForMe = notification.Status?.Mentions.Any(m => CalculateCommandSkips().Contains(m.AccountName)) ?? false;
            var isFrom = notification.Account.AccountName;
            log.LogDebug($"isFrom: {isFrom}");

            var isFavourited = notification.Status?.Favourited ?? false; // favourited == processed
            var isAuthorised = network.AuthorisedAccountsList.Contains(isFrom);
            var username = SocialUsername.From(
                notification.Account.AccountName,
                notification.Account.DisplayName,
                network);

            var inReplyTo = notification.Status!.InReplyToId;

            return new SocialCommand(
                receivingNetwork: network,
                username: username,
                postId: long.Parse(notification.Status!.Id),
                text: notification.Status!.Content,
                isForThisNode: isForMe,
                isAuthorised: isAuthorised,
                isRetrospective: isRetrospective,
                isProcessed: isFavourited,
                deliveryMedium: typeof(MastodonConnection).Name,
                deliveryType: notification.Type,
                inReplyTo: inReplyTo == null ? null : long.Parse(inReplyTo));
        }
    }
}
