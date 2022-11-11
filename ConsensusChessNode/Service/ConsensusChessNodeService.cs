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
                var games = db.Games
                    //.Include(g => g.Moves.Select(m => m.From).Select(b => b.BoardPosts))
                    //.Include(g => g.Moves.Select(m => m.To).Select(b => b.BoardPosts))
                    //.Include(g => g.Moves.Select(m => m.Votes))
                    .ToList();

                log.LogDebug(JsonConvert.SerializeObject(games));

                var unpostedBoardChecks = gm.FindUnpostedBoards(games, state.Shortcode);
                foreach (var check in unpostedBoardChecks)
                {
                    var game = check.Key;
                    var board = check.Value;

                    if (board != null)
                    {
                        log.LogInformation($"Found a new board to post in game: {game.Id}");

                        var posted = await social.PostAsync(game, board);

                        log.LogDebug("Saving board and new board posts...");


                        board.BoardPosts.Add(posted);
                        //db.Attach(posted);
                        //db.Games.Update(game);

                        log.LogTrace($"board.Id = {board.Id}");
                        log.LogTrace($"posted.Id = {posted.Id}");
                        log.LogTrace($"board.BoardPosts.Count = {board.BoardPosts.Count()}");
                        //db.Entry(posted).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        //db.Entry(board).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        //db.Entry(game).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

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

