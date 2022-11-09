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

        public MastodonConnection(ILogger log, Network network, NodeState state) : base(log, network, state)
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

        public override async Task<PostReport> PostToNetworkAsync(Post post)
        {
            try
            {
                var status = await client.PostStatus(post.Message, replyStatusId: post.ReplyTo);
                return PostReport.Success(post);
            }
            catch (Exception e)
            {
                return PostReport.From(e, post);
            }
        }

        public override async Task StartListeningForCommandsAsync(Func<SocialCommand, Task> asyncCommandReceiver, bool getMissedCommands)
        {
            asyncCommandReceivers += asyncCommandReceiver;

            // fetch conversations up to now before starting to stream
            log.LogDebug(
                getMissedCommands
                ? $"Retrieving any missed notifications since: {state.LastNotificationId}"
                : $"Skipping any missed commands.");
            var missedNotifications = getMissedCommands ? await client.GetNotifications(sinceId: state.LastNotificationId) : null;
            // TODO: paging

            // set up the stream
            stream = client.GetUserStreaming();
            stream.OnNotification += Stream_OnNotification;
            log.LogDebug("Starting stream");
            stream.Start(); // not awaited - awaiting blocks

            // process notifications already found
            if (missedNotifications != null)
            {
                log.LogDebug("Processing previously found notifications");
                missedNotifications.ForEach(async n => { await ProcessNotification(n, true); });
            }
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

            if (isMention && isForMe && !isFavourited)
            {
                // immediately update state
                var statusId = notification.Status!.Id;
                if (statusId > state.LastNotificationId)
                {
                    state.LastNotificationId = statusId;
                    await ReportStateChangeAsync();
                }

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

                // now favourite to mark the notification as dealt with
                await client.Favourite(notification.Status!.Id);

                // now invoke the command (even if this fails we wouldn't want to re-run)
                if (asyncCommandReceivers != null)
                {
                    await asyncCommandReceivers.Invoke(command);
                }
                else
                {
                    log.LogWarning("No receivers for this command.");
                }

                // TODO: catch errors, log, notify the user?
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

        public override IEnumerable<string> CalculateCommandSkips() => new[]
        {
            $"@{AccountName}",
            $"@{AccountName}@{network.NetworkServer}",
        };
    }
}
