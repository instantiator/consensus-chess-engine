using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
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
        public static int MAX_PAGES = 100;
        public static int RATE_LIMIT_PERMITTED_REQUESTS = 300;
        public static TimeSpan RATE_LIMIT_PERMITTED_PERIOD = TimeSpan.FromSeconds(300);

        public MastodonConnection(ILogger log, Network network, string shortcode, ServiceConfig config)
            : base(log, network, shortcode, RATE_LIMIT_PERMITTED_REQUESTS, RATE_LIMIT_PERMITTED_PERIOD, config)
		{
            client = new MastodonClient(network.NetworkServer, network.AppToken, http);
        }

        protected override async Task InitImplementationAsync()
        {
            await RateLimitAsync();
            user = await client.GetCurrentUser();

            var expectedUser = SocialUsername.From(network.ExpectedAccountName, user.DisplayName!, network, shortcode);
            Username = SocialUsername.From(user.AccountName!, user.DisplayName!, network, shortcode);

            if (expectedUser.Full != Username.Full)
            {
                throw new ArgumentException($"Expected to connect as: {expectedUser.Full}\nActually connected as: {Username.Full}");
            }
        }

        public override SocialUsername? Username { get; set; }

        public static Dictionary<PostType, Visibility> VisibilityMapping = new Dictionary<PostType, Visibility>()
        {
            { PostType.SocialStatus, Visibility.Private },

            { PostType.CommandResponse, Visibility.Direct },
            { PostType.CommandRejection, Visibility.Direct },

            { PostType.Engine_GameAnnouncement, Visibility.Unlisted },
            { PostType.Engine_GameAdvance, Visibility.Unlisted },
            { PostType.Engine_GameCreationResponse, Visibility.Direct },
            { PostType.Engine_GameAbandoned, Visibility.Unlisted },
            { PostType.Engine_GameEnded, Visibility.Unlisted },

            { PostType.Node_BoardUpdate, Visibility.Public },
            { PostType.Node_BoardReminder, Visibility.Public },
            { PostType.Node_VotingInstructions, Visibility.Public },
            { PostType.Node_FollowInstructions, Visibility.Public },
            { PostType.Node_GameAbandonedUpdate, Visibility.Public },
            { PostType.Node_GameEndedUpdate, Visibility.Public },

            { PostType.Node_VoteAccepted, Visibility.Direct },
            { PostType.Node_VoteSuperceded, Visibility.Direct },
            { PostType.Node_MoveValidation, Visibility.Direct },
            { PostType.Node_GameNotFound, Visibility.Direct },

            { PostType.Unspecified, Visibility.Unlisted },
        };

        protected override async Task<bool> UploadMediaImplementationAsync(Media media, bool? dryRun)
        {
            // already rate limited through the public method
            var dry = dryRun ?? dryRuns;
            try
            {
                if (!dry)
                {
                    log.LogDebug($"Uploading {media.Filename} to network...");
                    var stream = new MemoryStream(media.Data);
                    var attachment = await client.UploadMedia(stream, media.Filename, media.Alt);
                    media.SocialId = attachment.Id;
                    media.PreviewUrl = attachment.PreviewUrl;
                }
                else
                {
                    log.LogWarning($"Dry run of upload {media.Filename} to network.");
                    media.SocialId = Guid.NewGuid().ToString();
                    media.PreviewUrl = "http://placekitten.com/200/300";
                }
                return true;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unable to upload media: {media.Filename}");
                return false;
            }
        }

        protected override async Task<Post> PostImplementationAsync(Post post, bool? dryRun)
        {
            // already rate limited through the public method

            // get visibility and map to the config setting if it's a public post
            Visibility visibility = post.OverrideMastodonVisibility ?? VisibilityMapping[post.Type];
            visibility = visibility == Visibility.Public
                ? config.MastodonPublicPostVisibility
                : visibility;

            var dry = dryRun ?? dryRuns;
            try
            {
                if (!dry)
                {
                    var mediaIds = post.Media.Count() > 0
                        ? post.Media.Where(m => m.SocialId != null).Select(m => m.SocialId!)
                        : null;

                    log.LogDebug($"Posting to network...");
                    var status = await client.PublishStatus(
                        status: post.Message!,
                        visibility: visibility,
                        mediaIds: mediaIds,
                        replyStatusId: post.NetworkReplyToId?.ToString());

                    post.Succeed(state.Shortcode, network.AppName, network.NetworkServer, long.Parse(status.Id));
                }
                else
                {
                    log.LogWarning($"Dry run of post to network.");
                    post.Succeed(state.Shortcode, network.AppName, network.NetworkServer, null);
                }
            }
            catch (Exception e)
            {
                post.Fail(state.Shortcode, network.AppName, network.NetworkServer, e: e);
            }
            return post;
        }

        protected override async Task StartListeningForNotificationsAsync()
        {
            // set up the stream
            await RateLimitAsync();
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

        protected int recentIterations; // for exposure in tests
        protected override async Task<IEnumerable<Notification>> GetAllNotificationSinceAsync(long sinceId)
        {
            var list = new List<Notification>();
            long? nextPageMaxId = null;
            recentIterations = 0;

            do
            {
                ArrayOptions opts = new ArrayOptions()
                {
                    SinceId = sinceId.ToString(),
                    MaxId = nextPageMaxId?.ToString()
                };

                await RateLimitAsync();
                var page = await client.GetNotifications(options: opts);

                list.AddRange(page.Where(pn => !list.Select(n => n.Id).Contains(pn.Id)));
                nextPageMaxId = page.NextPageMaxId;
                recentIterations++;
            } while (nextPageMaxId != null && recentIterations < MAX_PAGES);
            return list.OrderBy(n => n.CreatedAt);
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
            await RateLimitAsync();
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
