using System;
using System.Text.RegularExpressions;
using Chess;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using Mastonet.Entities;

namespace ConsensusChessShared.Content
{
	public class MoveFormatter
	{
        public static readonly Regex CCF = new Regex("(?<from>[a-h][1-8])\\s?[-,]\\s?(?<to>[a-h][1-8])");

        public static readonly string Explanation =
            "Move should be written as two positions, separated by a hyphen: from - to, eg. 'e2 - e4'\n" +
            "Moves written in SAN (Standard Algebraic Notation) are also acceptable.";

        public static bool LooksLikeMoveText(Board context, string text)
        {
            var ccf = CCF.IsMatch(text.Trim().ToLower());
            if (ccf)
                return true;
            ChessBoard chessboard = ChessBoard.LoadFromFen(context.FEN);
            return chessboard.IsValidMove(text.Trim());
        }

        /// <summary>
        /// Attempts to create a Move using Consensus Chess Format (eg. "e2 - e4")
        /// </summary>
        /// <param name="text">move text to parse</param>
        /// <returns>A Chess.Move if it can be parsed from the text, or null</returns>
        public static Chess.Move? GetChessMoveFromCCF(string text)
        {
            var readyCCF = text.Trim().ToLower();
            if (!CCF.IsMatch(readyCCF)) { return null; }
            var matches = CCF.Matches(readyCCF);
            var from = matches.Single().Groups.Cast<Group>().Single(m => m.Name == "from").Value;
            var to = matches.Single().Groups.Cast<Group>().Single(m => m.Name == "to").Value;
            return new Chess.Move(from, to);
        }

        /// <summary>
        /// Attempts to create a Move using SAN - which means it needs a Board for context, too.
        /// </summary>
        /// <param name="board">Board from which to apply the SAN</param>
        /// <param name="text">SAN for a move</param>
        /// <returns>A Move (if it can be created), or null</returns>
        public static Chess.Move? GetChessMoveFromSAN(Board board, string text)
        {
            var readySAN = text.Trim();
            ChessBoard chessboard = ChessBoard.LoadFromFen(board.FEN);
            return chessboard.IsValidMove(readySAN)
                ? chessboard.ParseFromSan(readySAN)
                : null;
        }

    }
}

