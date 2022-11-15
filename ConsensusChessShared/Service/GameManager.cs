using System;
using Chess;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using Mastonet.Entities;
using Microsoft.EntityFrameworkCore;
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

        public Game CreateSimpleMoveLockGame(string gameShortcode, string gameDescription, IEnumerable<string>? participantNetworkServers, IEnumerable<string> postingNodeShortcodes)
        {
            return Game.NewGame(
                gameShortcode, gameDescription,
                participantNetworkServers, participantNetworkServers,
                postingNodeShortcodes, postingNodeShortcodes,
                SideRules.MoveLock);
        }

        public Dictionary<Game,Board?> FindUnpostedBoards(DbSet<Game> games, string postingNodeShortcode)
        {
            return games.ToList()
                .Where(g => UnpostedBoardOrNull(g, postingNodeShortcode) != null)
                .ToDictionary(g => g, g => UnpostedBoardOrNull(g, postingNodeShortcode));
        }
        
        public Board? UnpostedBoardOrNull(Game game, string postingNodeShortcode)
        {
            if (!game.Active) { return null; }

            // examine game - get for currently playing side (shortcodes)
            var isThisNetwork = game.CurrentMove.SideToPlay == Side.White ?
                game.WhitePostingNodeShortcodes.Contains(postingNodeShortcode) :
                game.BlackPostingNodeShortcodes.Contains(postingNodeShortcode);

            if (!isThisNetwork) { return null; }

            // if shortcode found amongst currently playing, check to see if there's already a post
            var board = game.CurrentMove.From;
            var alreadyPosted = board.BoardPosts.Any(bp => bp.NodeShortcode == postingNodeShortcode);

            // if not already posted, then return the board, otherwise null
            return alreadyPosted
                ? null
                : game.CurrentMove.From;
        }

        /// <summary>
        /// Validates the move, and throws a MoveRejectionException if there's an issue
        /// </summary>
        /// <param name="submission">the move (ideally in SAN format)</param>
        /// <returns>The new board position, if the move was valid</returns>
        /// <exception cref="MoveRejectionException"></exception>
        public Board ValidateSAN(Board board, string tryMoveSAN)
        {
            try
            {
                //var move = new Chess.Move(tryMoveSAN);
                var chessboard = ChessBoard.LoadFromFen(board.FEN);
                var ok = chessboard.Move(tryMoveSAN);
                if (!ok) { throw new VoteRejectionException(VoteValidationState.InvalidSAN, message: $"Move cannot be made."); }
                return Board.FromFEN(chessboard.ToFen());
            }
            catch (ChessException e)
            {
                // TODO: better rejection content
                throw new VoteRejectionException(VoteValidationState.InvalidSAN, message: $"{e.GetType().Name}: {e.Message}", innerException: e);
            }
        }

        public bool ParticipantMayVote(Game game, Participant participant)
        {
            switch (game.SideRules)
            {
                case SideRules.FreeForAll:
                    return true; // anybody can play

                case SideRules.ServerLock:
                    return game.CurrentParticipantNetworkServers.Contains(participant.NetworkServer);

                case SideRules.MoveLock:
                    var commitment = participant.Commitments.SingleOrDefault(c => c.GameShortcode == game.Shortcode);
                    return commitment == null || commitment.GameSide == game.CurrentSide;

                default:
                    throw new NotImplementedException(game.SideRules.ToString());
            }
        }
    }
}
