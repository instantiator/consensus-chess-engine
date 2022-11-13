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

        private event Func<SocialCommand, Task>? asyncCommandReceivers;
        private TimelineStreaming? stream;

        private const int MAX_PAGES = 100; // TODO: revisit this limit

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

        public override async Task StartListeningForCommandsAsync(Func<SocialCommand, Task> asyncCommandReceiver, bool getMissedCommands)
        {
            asyncCommandReceivers += asyncCommandReceiver;

            // fetch conversations up to now before starting to stream
            var firstStart = state.LastNotificationId == 0;

            log.LogDebug(
                getMissedCommands && !firstStart
                    ? $"Retrieving any missed notifications since: {state.LastNotificationId}"
                    : $"Skipping any missed commands.");
            var missedNotifications =
                getMissedCommands && !firstStart
                    ? await GetAllNotificationSince(state.LastNotificationId)
                    : null;

            // set up the stream
            stream = client.GetUserStreaming();
            stream.OnNotification += Stream_OnNotification;
            log.LogDebug("Starting stream");
            stream.Start(); // not awaited - awaiting blocks

            // retrospectively process notifications already found
            if (missedNotifications != null)
            {
                log.LogDebug("Retrospectively processing previously found notifications...");
                foreach (var missedNotification in missedNotifications)
                {
                    await ProcessNotification(missedNotification, true);
                }
            }
        }

        private async Task<IEnumerable<Notification>> GetAllNotificationSince(long sinceId)
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
            await ProcessNotification(e.Notification, false);
        }

        private async Task ProcessNotification(Notification notification, bool retrospective)
        {
            log.LogDebug($"Processing notification, type: {notification.Type}, retrospectively: {retrospective}");

            var isMention = notification.Type == "mention";
            var isForMe = notification.Status?.Mentions.Any(m => m.AccountName == AccountName) ?? false;
            var isFrom = notification.Status?.Account.AccountName;
            var isFavourited = notification.Status?.Favourited ?? false; // favourited == seen
            var isAuthorised = network.AuthorisedAccountsList.Contains(isFrom);

            // only log the mentions
            if (isMention)
            {
                log.LogDebug($"isMention: {isMention}, isForMe: {isForMe}, isAuthorised: {isAuthorised}, isFavourited: {isFavourited}");
            }

            // favourite to mark the status as seen - we won't try again, even if execution fails
            if (isForMe && notification.Status != null && !isFavourited)
            {
                await client.Favourite(notification.Status.Id);
            }

            // always update the last notification id
            var statusId = notification.Status!.Id;
            if (statusId > state.LastNotificationId)
            {
                state.LastNotificationId = statusId;
                await ReportStateChangeAsync();
            }

            if (isMention && isForMe && !isFavourited)
            {
                // now process command
                var command = new SocialCommand()
                {
                    Network = network,
                    NetworkUserId = notification.Status!.Account.AccountName,
                    RawText = notification.Status!.Content,
                    SourceId = notification.Status!.Id,
                    IsAuthorised = isAuthorised,
                    IsRetrospective = retrospective
                };

                // now invoke the command (even if this fails we wouldn't want to re-run)
                if (asyncCommandReceivers != null)
                {
                    await asyncCommandReceivers.Invoke(command);
                }
                else
                {
                    log.LogWarning("No receivers for this command.");
                }
            }
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
    }
}
