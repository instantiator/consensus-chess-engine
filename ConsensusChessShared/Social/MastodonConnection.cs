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
        private long lastCommandId = 0;

        public MastodonConnection(ILogger log, Network network) : base(log, network)
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

        protected override async Task<PostReport> PostToNetworkAsync(string text)
        {
            try
            {
                var status = await client.PostStatus(text);
                return PostReport.Success();
            }
            catch (Exception e)
            {
                return PostReport.From(e);
            }
        }

        public override async Task StartListeningForCommandsAsync(Func<SocialCommand, Task> asyncCommandReceiver, long? sinceId = null)
        {
            asyncCommandReceivers += asyncCommandReceiver;

            // fetch conversations up to now, and then start streaming
            var notifications = await client.GetNotifications(sinceId);
            // TODO: paging

            // set up the stream
            stream = client.GetUserStreaming();
            stream.OnNotification += Stream_OnNotification;
            log.LogDebug("Starting stream");
            stream.Start(); // not awaited - awaiting blocks

            // process notifications already found
            log.LogDebug("Processing previously found notifications");
            notifications.ForEach(async n => { await ProcessNotification(n); });
        }

        private async void Stream_OnNotification(object? sender, StreamNotificationEventArgs e)
        {
            await ProcessNotification(e.Notification);
        }

        private async Task ProcessNotification(Notification notification)
        {
            log.LogDebug($"Processing notification, type: {notification.Type}");

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
                var command = new SocialCommand()
                {
                    Network = network,
                    NetworkUserId = notification.Status!.Account.AccountName,
                    RawText = notification.Status!.Content,
                    SourceId = notification.Status!.Id,
                    IsAuthorised = isAuthorised
                };

                var statusId = notification.Status!.Id;
                lastCommandId = statusId > lastCommandId ? statusId : lastCommandId;

                // TODO: update database with last command id

                if (asyncCommandReceivers != null)
                {
                    await asyncCommandReceivers.Invoke(command);
                }

                await client.Favourite(notification.Status!.Id); // favourite to indicate this is dealt with
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
