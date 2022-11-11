using System;
using System.Collections;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ConsensusChessNode.Service
{
    public class ConsensusChessNodeService : AbstractConsensusService
    {
        protected override TimeSpan PollPeriod => TimeSpan.FromSeconds(15);
        protected override NodeType NodeType => NodeType.Node;

        public ConsensusChessNodeService(ILogger log, IDictionary env) : base(log, env)
        {
        }

        protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            using (var db = GetDb())
            {
                var games = db.Games.ToList();
                var unpostedBoardChecks = gm.FindUnpostedBoards(games, state.Shortcode);

                // log the unposted boards for debugging purposes
                if (unpostedBoardChecks.Count() > 0)
                {
                    log.LogTrace(JsonConvert.SerializeObject(games));
                }

                foreach (var check in unpostedBoardChecks)
                {
                    var game = check.Key;
                    var board = check.Value;

                    if (board != null)
                    {
                        log.LogInformation($"Found a new board to post in game: {game.Id}");
                        var posted = await social.PostAsync(game, board);
                        board.BoardPosts.Add(posted);

                        log.LogDebug("Saving board and new board posts...");
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        protected override void RegisterForCommands(CommandProcessor processor)
        {
            processor.Register("shutdown", requireAuthorised: true, runsRetrospectively: false, ShutdownAsync);
        }

        private async Task ShutdownAsync(SocialCommand origin, IEnumerable<string> words)
        {
            log.LogInformation($"Shutting down.");
            polling = false;
            pollingCancellation.Cancel();
        }

        protected override async Task FinishAsync()
        {
            log.LogDebug("FinishAsync");
        }

    }
}

