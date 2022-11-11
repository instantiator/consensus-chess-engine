using System;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Social
{
	public abstract class AbstractSocialConnection : ISocialConnection
	{
		public event Func<NodeState, Task> OnStateChange;

		protected ILogger log;
		protected Network network;
		protected NodeState state;
		protected bool dryRuns;

		public AbstractSocialConnection(ILogger log, Network network, NodeState state, bool dryRuns)
		{
			this.log = log;
			this.network = network;
			this.state = state;
			this.dryRuns = dryRuns;
		}

		public abstract Task InitAsync();
		public abstract string DisplayName { get; }
		public abstract string AccountName { get; }
		public abstract IEnumerable<string> CalculateCommandSkips();
		public abstract Task StartListeningForCommandsAsync(Func<SocialCommand, Task> asyncReceiver, bool retrieveMissedCommands);
		public abstract Task StopListeningForCommandsAsync(Func<SocialCommand, Task> asyncReceiver);

		public async Task<Post> PostAsync(SocialStatus status, bool? dryRun = null)
			=> await PostAsync($"{state.Name} ({state.Shortcode}): {status}", PostType.SocialStatus, dryRun);

		public async Task<Post> PostAsync(Game game, bool? dryRun = null)
			=> await PostAsync(
				string.Format("New {0} game...\nWhite: {1}\nBlack: {2}\nMove duration: {3}",
					game.SideRules,
                    string.Join(", ", game.WhiteNetworks),
                    string.Join(", ", game.BlackNetworks),
                    game.MoveDuration),
                PostType.EngineUpdate,
                dryRun);

        public async Task<Post> PostAsync(Game game, Board board, bool? dryRun = null)
            => await PostAsync(
                string.Format("New board. You have {1} to vote.\n{0}",
                    BoardFormatter.PiecesFENtoVisualEmoji(board.Pieces_FEN),
					game.MoveDuration.ToString()),
                PostType.BoardUpdate,
				dryRun);

        public async Task<Post> PostAsync(string text, PostType type = PostType.Unspecified, bool? dryRun = null)
		{
			log.LogInformation($"Posting: {text}");

			var post = new Post()
			{
				Message = text,
				NodeShortcode = state.Shortcode,
                NetworkServer = network.NetworkServer,
                AppName = network.AppName,
                Type = type
            };

            return await PostToNetworkAsync(post, dryRun ?? dryRuns);
		}

		public async Task<Post> ReplyAsync(SocialCommand origin, string message, bool? dryRun = null)
		{
            log.LogInformation($"Replying: {message}");

            var post = new Post()
            {
                Message = message,
                NodeShortcode = state.Shortcode,
                NetworkServer = network.NetworkServer,
                AppName = network.AppName,
                Type = PostType.TextResponse,
				ReplyTo = origin.SourceId
            };

            return await PostToNetworkAsync(post, dryRun ?? dryRuns);
        }

        public abstract Task<Post> PostToNetworkAsync(Post post, bool dryRun);

		protected async Task ReportStateChangeAsync()
		{
			log.LogDebug($"ReportStateChange - new notification id: {state.LastNotificationId}");
			if (OnStateChange != null)
				await OnStateChange.Invoke(state);
		}

    }
}

