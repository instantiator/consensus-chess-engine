using System;
using System.Collections;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Social;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Service
{
	public class DbOperator
	{
        public const string SHORTCODE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public const int SHORTCODE_LENGTH = 6;

        private ILogger log;
        private IDictionary env;

		public DbOperator(ILogger log, IDictionary env)
		{
            this.log = log;
            this.env = env;
		}

        public ConsensusChessDbContext GetDb() => ConsensusChessDbContext.FromEnvironment(env);

        /// <summary>
        /// Determines the participant that created the post - or creates said participant.
        /// </summary>
        /// <param name="post"></param>
        /// <returns>A copy of the new participant</returns>
        public async Task<Participant> FindOrCreateParticipantAsync(ConsensusChessDbContext db, SocialCommand post)
        {
            var participant = db.Participant.SingleOrDefault(p => p.NetworkUserAccount == post.SourceAccount);
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
        /// Retrieves the game with the board post that this vote is in response to - or fails.
        /// </summary>
        /// <param name="cmd">the social command to check</param>
        /// <returns>the game that this social command refers to (if any)</returns>
        /// <exception cref="GameNotFoundException"></exception>
        public Game GetGameForVote(ConsensusChessDbContext db, SocialCommand cmd)
        {
            // check if the reply is directly to a current board
            var game = db.Games.ToList()
                .SingleOrDefault(g => g.CurrentBoard.BoardPosts
                    .Any(bp => bp.NetworkPostId == cmd.InReplyToId));

            if (game == null)
            {
                throw new GameNotFoundException(cmd, "Game not found");
            }

            return game;
        }

        public string GenerateUniqueGameShortcode(ConsensusChessDbContext db)
        {
            var shortcodes = db.Games.Select(g => g.Shortcode);
            var random = new Random();
            string shortcode;
            do
            {
                shortcode = new string(Enumerable
                    .Repeat(SHORTCODE_CHARS, SHORTCODE_LENGTH)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (shortcodes.Contains(shortcode));
            return shortcode;
        }

    }
}

