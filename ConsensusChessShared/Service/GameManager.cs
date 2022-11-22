using System;
using Chess;
using ConsensusChessShared.Constants;
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

        public Dictionary<Game,Board?> FindUnpostedActiveBoards(DbSet<Game> games, string postingNodeShortcode)
        {
            return games.ToList()
                .Where(g => g.Active && UnpostedBoardOrNull(g, postingNodeShortcode) != null)
                .ToDictionary(g => g, g => UnpostedBoardOrNull(g, postingNodeShortcode));
        }
        
        public Board? UnpostedBoardOrNull(Game game, string postingNodeShortcode)
        {
            if (!game.Active) { return null; }

            // examine game - get for currently playing side (shortcodes)
            var isThisNetwork = game.CurrentMove.SideToPlay == Side.White ?
                game.WhitePostingNodeShortcodes.Contains(new StoredString(postingNodeShortcode)) :
                game.BlackPostingNodeShortcodes.Contains(new StoredString(postingNodeShortcode));

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
        public Board ValidateSAN(Board board, Vote vote)
        {
            try
            {
                //var move = new Chess.Move(tryMoveSAN);
                var chessboard = ChessBoard.LoadFromFen(board.FEN);
                var ok = chessboard.Move(vote.MoveText);
                if (!ok) { throw new VoteRejectionException(vote, VoteValidationState.InvalidSAN); }
                return Board.FromFEN(chessboard.ToFen());
            }
            catch (ChessException e)
            {
                // TODO: better rejection content
                throw new VoteRejectionException(vote, VoteValidationState.InvalidSAN, detail: $"{e.GetType().Name}: {e.Message}", innerException: e);
            }
        }

        /// <summary>
        /// Checks if the participant may vote on game provided, based on the side rules.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="participant"></param>
        /// <returns>true if the particpant matches the current side rule for the game</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool ParticipantOnSide(Game game, Participant participant)
        {
            switch (game.SideRules)
            {
                case SideRules.FreeForAll:
                    return true; // anybody can play

                case SideRules.ServerLock:
                    return game.CurrentParticipantNetworkServers.Contains(new StoredString(participant.NetworkServer));

                case SideRules.MoveLock:
                    var commitment = participant.Commitments.SingleOrDefault(c => c.GameShortcode == game.Shortcode);
                    return commitment == null || commitment.GameSide == game.CurrentSide;

                default:
                    throw new NotImplementedException(game.SideRules.ToString());
            }
        }

        /// <summary>
        /// Retrieves the participant's current, valid vote (or null) on the given move.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="participant"></param>
        /// <returns>Their Vote or null</returns>
        public Vote? GetCurrentValidVote(DTO.Move move, Participant participant)
        {
            return move.Votes.SingleOrDefault(v =>
                v.Participant.Id == participant.Id &&
                v.ValidationState == VoteValidationState.Valid);
        }

        /// <summary>
        /// Counts the numbers of distinct votes.
        /// </summary>
        /// <param name="move">The move to analyse</param>
        /// <returns>A dictionary of move SAN to count</returns>
        public Dictionary<string, int> CountVotes(DTO.Move move)
        {
            // Assume that the SAN is canonical. See: ICG-66 canonical SAN
            return move.Votes
                .Where(v => v.ValidationState == VoteValidationState.Valid)
                .Select(v => v.MoveText)
                .Distinct()
                .ToDictionary(dv => dv, dv => move.Votes.Count(v => v.MoveText == dv));
        }

        /// <summary>
        /// Determines the next move - either the most popular vote, or (in case of a tie) a random choice between the most popular votes.
        /// </summary>
        /// <param name="votes"></param>
        /// <returns>SAN for the next move or null if there were 0 votes</returns>
        public string? NextMoveFor(Dictionary<string,int> votes)
        {
            if (votes.Count() == 0) { return null; }
            var maxVote = votes.Max(pair => pair.Value);
            var mostPopularSAN = votes.Where(pair => pair.Value == maxVote).Select(v => v.Key);
            if (mostPopularSAN.Count() == 1)
            {
                return mostPopularSAN.Single();
            }
            else
            {
                var random = new Random();
                return mostPopularSAN.ElementAt(random.Next(mostPopularSAN.Count()-1));
            }
        }

        public void AdvanceGame(Game game, string move)
        {
            var newBoard = CreateNextBoard(game.CurrentBoard, move);

            // update game moves list
            game.CurrentMove.To = newBoard;
            game.CurrentMove.SelectedSAN = move;
            game.Moves.Add(new DTO.Move()
            {
                Deadline = DateTime.Now.Add(game.MoveDuration).ToUniversalTime(),
                From = newBoard
            });
        }

        public Board CreateNextBoard(Board board, string SAN)
        {
            var chessboard = ChessBoard.LoadFromFen(board.FEN);
            chessboard.Move(SAN);
            return Board.FromFEN(chessboard.ToFen());
        }

        public void AbandonGame(Game game)
        {
            game.State = GameState.Abandoned;
            game.Finished = DateTime.Now.ToUniversalTime();
        }
    }
}
