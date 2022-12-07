using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using Mastonet.Entities;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public abstract class AbstractSocialConnection : ISocialConnection
	{
		public event Func<NodeState, Task>? OnStateChange;

        public bool Ready { get; private set; }
        public bool Streaming { get; protected set; }
        public bool Paused { get; protected set; }

        protected int socialActionRetries = 3;
        protected TimeSpan socailActionRetryDelay = TimeSpan.FromSeconds(2);

		protected ILogger log;
		protected Network network;
		protected NodeState? state;
		protected bool dryRuns;
        protected string shortcode;
        protected ServiceConfig config;

        protected event Func<SocialCommand, Task>? asyncCommandReceivers;
        protected IEnumerable<SocialCommand>? missedCommands;

        protected RateLimiter? rateLimiter;

        public AbstractSocialConnection(ILogger log, Network network, string shortcode, int? permittedRequests, TimeSpan? rateLimitPeriod, ServiceConfig config)
		{
			this.log = log;
			this.network = network;
			this.dryRuns = network.DryRuns;
            this.Ready = false;
            this.shortcode = shortcode;
            this.config = config;

            Streaming = false;

            if (permittedRequests != null && rateLimitPeriod != null)
                this.rateLimiter = new RateLimiter(log, permittedRequests.Value, rateLimitPeriod.Value);
		}

		public async Task InitAsync(NodeState state)
        {
            this.state = state;
            await InitImplementationAsync();
            Ready = true;
        }

        protected abstract Task InitImplementationAsync();

        public abstract SocialUsername? Username { get; set; }

		public abstract IEnumerable<string> CalculateCommandSkips();
        protected abstract Task GetMissedCommands();
        protected abstract Task MarkCommandProcessedAsync(string id);
        public abstract Task<IEnumerable<SocialCommand>> GetAllNotificationSinceAsync(bool isRetrospective, string? sinceId, DateTime? orSinceWhen = null);

        protected abstract Task StopStreamingNotificationsAsync();
        public async Task StopListeningForCommandsAsync(Func<SocialCommand, Task> asyncReceiver)
        {
            if (config.StreamEnabled)
                await StopStreamingNotificationsAsync();

            asyncCommandReceivers -= asyncReceiver;
        }

        public void PauseStream() { Paused = true; }
        public void ResumeStream() { Paused = false; }

        protected abstract Task StartStreamingNotificationsAsync();
        public async Task StartListeningForCommandsAsync(Func<SocialCommand, Task> asyncCommandReceiver, bool getMissedCommands)
		{
            asyncCommandReceivers += asyncCommandReceiver;
            if (getMissedCommands) { await GetMissedCommands(); }

            if (config.StreamEnabled)
                await StartStreamingNotificationsAsync();

            if (getMissedCommands) { await ProcessMissedCommands(); }
        }

        protected async Task ProcessMissedCommands()
        {
            if (missedCommands != null)
            {
                log.LogDebug($"Retrospectively processing {missedCommands.Count()} notifications...");
                foreach (var command in missedCommands)
                {
                    await ProcessCommandAsync(command);
                }
            }
        }

        public async Task ProcessCommandAsync(SocialCommand command)
        {
            log.LogDebug(
                $"Processing {command.DeliveryMedium}:{command.DeliveryType} command." + "{0}",
                command.IsRetrospective ? " (retrospectively)" : "");

            log.LogDebug($"IsForMe: {command.IsForThisNode}, IsAuthorised: {command.IsAuthorised}, IsProcessed: {command.IsProcessed}");

            try
            {
                if (command.IsForThisNode && !command.IsProcessed)
                {
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
                else
                {
                    log.LogWarning("Command received, but not for this node.");
                }
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unexpected exception processing command: {command.RawText}");
            }
            finally
            {

                // always mark the status as seen - we won't try again, even if execution fails
                if (command.IsForThisNode && !command.IsProcessed)
                {
                    await MarkCommandProcessedAsync(command.SourcePostId);
                    command.IsProcessed = true;
                }

                // update the last status id
                if (state!.LastCommandStatusId == null ||
                    ulong.Parse(command.SourcePostId) > ulong.Parse(state!.LastCommandStatusId))
                {
                    state.LastCommandStatusId = command.SourcePostId;
                }

                // update the last notification id (if present)
                if (command.SourceNotificationId != null)
                {
                    if (state!.LastNotificationId == null ||
                    ulong.Parse(command.SourceNotificationId!) > ulong.Parse(state!.LastNotificationId))
                    {
                        state.LastNotificationId = command.SourceNotificationId;
                    }
                }
                await ReportStateChangeAsync();
            }
        }

        public async Task<Post> PostAsync(Post post, bool? dryRun = null)
        {
            int mediaUploads = 0, uploadFailures = 0;
            foreach (var media in post.Media)
            {
                await RateLimitAsync();
                var ok = await UploadMediaImplementationAsync(media, dryRun);
                if (ok) { mediaUploads++; } else { uploadFailures++; }
            }

            if (mediaUploads > 0)
                log.LogInformation($"{mediaUploads} successful media uploads.");
            if (uploadFailures > 0)
                log.LogError($"{uploadFailures} failed media uploads.");

            await RateLimitAsync();
            var posted = await PostImplementationAsync(post, dryRun);

            return posted;
        }

        protected async Task RateLimitAsync()
        {
            if (rateLimiter != null)
                await rateLimiter.RateLimitAsync();
        }

        protected abstract Task<bool> UploadMediaImplementationAsync(Media media, bool? dryRun);
        protected abstract Task<Post> PostImplementationAsync(Post post, bool? dryRun);

        protected async Task ReportStateChangeAsync()
		{
			log.LogDebug($"Last notification: {state!.LastNotificationId}, last status: {state!.LastCommandStatusId}");
			if (OnStateChange != null)
				await OnStateChange.Invoke(state);
		}


        protected async Task<T> RetryWithDelayAndGetAsync<T>(Func<Task<T>> function, int? retries = null, TimeSpan? delay = null)
        {
            retries = retries ?? socialActionRetries;
            delay = delay ?? socailActionRetryDelay;
            int attempts = 0;
            bool success = false;
            T? result = default(T);
            do
            {
                try
                {
                    attempts++;
                    result = await function();
                    success = true;
                }
                catch (Exception e)
                {
                    log.LogWarning($"{e.GetType().Name} caught during social action, attempt {attempts}: {e.Message}");
                    if (attempts == retries)
                        throw;
                }
                if (!success)
                {
                    await Task.Delay(delay.Value);
                }
            }
            while (attempts < retries && !success);
            if (!success)
            {
                // this is defensive - if it failed, it would be because of an exception thrown during execution
                throw new Exception($"Failed to execute social action {retries} times.");
            }
            else
            {
                return result!;
            }
        }

        protected async Task RetryWithDelayAsync(Func<Task> function, int? retries = null, TimeSpan? delay = null)
        {
            retries = retries ?? socialActionRetries;
            delay = delay ?? socailActionRetryDelay;

            int attempts = 0;
            bool success = false;
            do
            {
                try
                {
                    attempts++;
                    await function();
                    success = true;
                }
                catch (Exception e)
                {
                    log.LogWarning($"{e.GetType().Name} caught during social action, attempt {attempts}: {e.Message}");
                    if (attempts == retries)
                        throw;
                }
                if (!success)
                {
                    await Task.Delay(delay.Value);
                }
            }
            while (attempts < retries && !success);
            if (!success)
            {
                // this is defensive - if it failed, it would be because of an exception thrown during execution
                throw new Exception($"Failed to execute social action {attempts} times.");
            }
        }
    }
}

