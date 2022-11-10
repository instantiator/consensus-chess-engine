using System;
using System.Collections;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;

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
            log.LogTrace("Polling...");
            using (var db = GetDb())
            {
                var games = db.Games.ToList();
                var unpostedBoardChecks = gm.FindUnpostedBoards(games, state.Shortcode);
                foreach (var check in unpostedBoardChecks)
                {
                    if (check.Value != null)
                    {
                        log.LogInformation($"Found a new board to post in game: {check.Key.Id}");
                        var posted = await social.PostAsync(check.Key, check.Value);

                        db.Add(posted);
                        check.Value.BoardPosts.Add(posted);
                        db.Update(check.Value);
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
            log.LogWarning("FinishAsync not implemented");
        }

    }
}

