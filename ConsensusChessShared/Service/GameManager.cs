using System;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using Microsoft.Extensions.Logging;

namespace ConsensusChessShared.Service
{
    public class GameManager
    {
        private ILogger log;

        public GameManager(ILogger log)
        {
            this.log = log;
        }

        public Game CreateSimpleMoveLockGame(IEnumerable<string> shortcodes)
            => Game.NewGame(shortcodes, shortcodes, SideRules.MoveLock);

        public Dictionary<Game,Board?> FindUnpostedBoards(IEnumerable<Game> games, string shortcode)
        {
            return games.ToDictionary(g => g, g => UnpostedBoardOrNull(g, shortcode));
        }
        
        public Board? UnpostedBoardOrNull(Game game, string shortcode)
        {
            if (!game.Active) { return null; }

            // TODO check if the board (should have been and) has been posted...


            // examine game - get for currently playing side (shortcodes)
            var isThisNetwork = game.CurrentMove.SideToPlay == Side.White ?
                game.WhiteNetworks.Contains(shortcode) :
                game.BlackNetworks.Contains(shortcode);

            if (!isThisNetwork) { return null; }

            // if shortcode found amongst currently playing, check to see if there's already a post
            var board = game.CurrentMove.From;
            var alreadyPosted = board.BoardPosts.Any(bp => bp.NodeShortcode == shortcode);

            // if not already posted, then return the board, otherwise null
            return alreadyPosted
                ? null
                : game.CurrentMove.From;
        }
    }
}

