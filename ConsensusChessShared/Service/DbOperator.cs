using System;
using System.Collections;
using System.IO;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Service
{
	public class DbOperator
	{
        public const string SHORTCODE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public const int SHORTCODE_LENGTH = 6;

        protected ILogger log;
        protected IDictionary? env;

		public DbOperator(ILogger log, IDictionary? env)
		{
            this.log = log;
            this.env = env;
		}

        public virtual ConsensusChessDbContext GetDb() => ConsensusChessPostgresContext.FromEnv(env!);

        public void InitDb(ConsensusChessDbContext db)
        {
            var connection = db.Database.CanConnect();
            log.LogDebug($"Database connection: {connection}");

            log.LogDebug($"Running migrations...");
            db.Database.Migrate();
        }

        /// <summary>
        /// Determines the participant that created the post - or creates said participant.
        /// </summary>
        /// <param name="post"></param>
        /// <returns>A copy of the new participant</returns>
        public async Task<Participant> FindOrCreateParticipantAsync(ConsensusChessDbContext db, SocialCommand post)
        {
            var participant = db.Participant.ToList().SingleOrDefault(p => p.Username.Full == post.SourceUsername.Full);
            log.LogDebug(participant == null ? "Participant not found, creating new" : "Participant found");
            if (participant == null)
            {
                participant = Participant.From(post);
                db.Participant.Add(participant);
                await db.SaveChangesAsync();
            }
            return participant;
        }

        /// <summary>
        /// Retrieves the game with the board post that this vote is in response to - or throws an exception.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="shortcode">shortcode for the current node</param>
        /// <param name="cmd">the social command to check</param>
        /// <returns>the game that this social command refers to</returns>
        /// <exception cref="GameNotFoundException">if the post is not in reply to a current board in any game</exception>
        public Game GetActiveGameForCurrentBoardResponse(ConsensusChessDbContext db, string shortcode, SocialCommand cmd)
        {
            // check if this is in reply to any boardpost from any game
            var gamesForNode = GetActiveGamesForNode(db, shortcode);

            var game = db.Games.ToList()
                .SingleOrDefault(g => g.Moves.Any(m =>
                    m.From.BoardPosts.Any(bp =>
                        bp.NetworkPostId == cmd.InReplyToId)));

            if (game == null)
                throw new GameNotFoundException(cmd, GameNotFoundReason.NoLinkedGame);

            var currentBoardPost = game.CurrentBoard.BoardPosts
                .SingleOrDefault(bp => bp.NetworkPostId == cmd.InReplyToId);

            if (currentBoardPost == null)
                throw new GameNotFoundException(cmd, GameNotFoundReason.BoardReferenceExpired);

            return game;
        }

        /// <summary>
        /// Returns the active games that the specified node should care about.
        /// </summary>
        /// <param name="shortcode">node shortcode (or null to match any)</param>
        /// <returns></returns>
        public IEnumerable<Game> GetActiveGamesForNode(ConsensusChessDbContext db, string? shortcode)
        {
            return db.Games.ToList()
                .Where(g => g.Active)
                .Where(g => shortcode == null || g.CurrentParticipantNetworkServers.Any(ss => ss.Value == shortcode));
        }

        /// <summary>
        /// Identifies currently active games with current move expired, for the current node.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="shortcode">shortcode</param>
        /// <returns>a collection of active games</returns>
        public IEnumerable<Game> GetActiveGamesWithExpiredMoves(ConsensusChessDbContext db, string? shortcode)
        {
            return GetActiveGamesForNode(db, shortcode).Where(game => game.CurrentMove.Expired);
        }

        /// <summary>
        /// Generates a unique shortcode for a game (checks that it doesn't appear in the db already).
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public string GenerateUniqueGameShortcode(ConsensusChessDbContext db)
        {
            var shortcodes = db.Games.Select(g => g.Shortcode);
            var random = new Random();
            string shortcode;
            do
            {
                shortcode = RandomShortcode();
            } while (shortcodes.Contains(shortcode));
            return shortcode;
        }

        public static Random DbOperatorRandom = new Random();

        public static string RandomShortcode()
        =>
            new string(Enumerable
                    .Repeat(SHORTCODE_CHARS, SHORTCODE_LENGTH)
                    .Select(s => s[DbOperatorRandom.Next(s.Length)]).ToArray());
        
        public async Task WipeDataAsync(ConsensusChessDbContext db)
        {
            var dbTables = db.Model.GetEntityTypes()
                .SelectMany(t => t.GetTableMappings())
                .Select(m => m.Table.Name)
                .Distinct()
                .ToList();

            foreach (var table in dbTables)
            {
                var sql = $"DELETE FROM {table};";
                log.LogDebug(sql);
                await db.Database.ExecuteSqlRawAsync(sql);
            }

        }

    }
}

