using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Social;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace ConsensusChessShared.Content
{
	public class EnumTranslator
	{
		public string Translate(SocialStatus status)
			=> status.ToString();

        public string Translate(GameState state)
        {
            switch (state)
            {
                case GameState.InProgress:
                    return "in progress";

                case GameState.WhiteKingCheckmated:
                    return "white king in checkmate";

                case GameState.BlackKingCheckmated:
                    return "black king in checkmate";

                case GameState.Stalemate:
                    return "stalemate";

                case GameState.Abandoned:
                    return "abandoned";

                default:
                    throw new NotImplementedException($"GameState.{state} not translated.");
            }
        }

        public string Translate(SideRules rules)
        {
            switch (rules)
            {
                case SideRules.FreeForAll:
                    return "free-for-all";

                case SideRules.MoveLock:
                    return "move-lock";

                case SideRules.ServerLock:
                    return "server-vs-server";

                default:
                    throw new NotImplementedException($"SideRules.{rules} not translated.");
            }    
        }

        public string Explain(SideRules rules)
        {
            switch (rules)
            {
                case SideRules.FreeForAll:
                    return "Anybody can play for either side at any time.";

                case SideRules.MoveLock:
                    return "Choose your side by voting.";

                case SideRules.ServerLock:
                    return "Your side is determined by your server.";

                default:
                    throw new NotImplementedException($"SideRules.{rules} not translated.");
            }
        }

        public string Describe(VoteValidationState validation)
        {
			switch (validation)
			{
				case VoteValidationState.Unchecked:
					return "This vote has not been checked yet.";

                case VoteValidationState.InvalidMoveText:
					return "The move provided could not be interpreted as a valid move.";

                case VoteValidationState.NoGame:
					return "This vote could not be linked to a game.";

				case VoteValidationState.OffSide:
					return "You are already registered to play for the other side.";

				case VoteValidationState.Superceded:
					return "This vote has been superceded.";

                case VoteValidationState.Valid:
					return "This vote is valid.";

				default:
					throw new NotImplementedException($"VoteValidationState.{validation} not translated.");
            }
        }

        public string Describe(CommandRejectionReason reason)
        {
			switch (reason)
			{
				case CommandRejectionReason.UnrecognisedCommand:
                    return "This command was not recognised.";

                case CommandRejectionReason.NotAuthorised:
                    return "You are not authorised to issue this command. Please contact an administrator.";

                case CommandRejectionReason.NoCommandWords:
                    return "This command is empty.";

                case CommandRejectionReason.CommandMalformed:
                    return "There was an issue with the format or parameters of this command.";

                case CommandRejectionReason.UnexpectedException:
                    return "An unexpected exception occurred whilst executing this command.";

                case CommandRejectionReason.NotForRetrospectiveExecution:
					return "This command is now stale. Please resend to issue it again.";

                default:
                    throw new NotImplementedException($"CommandRejectionReason.{reason} not translated.");
            }
        }

        public string Describe(GameNotFoundReason reason)
        {
            switch (reason)
            {
                case GameNotFoundReason.NoLinkedGame:
                    return "No game is linked to this post.";

                case GameNotFoundReason.BoardReferenceExpired:
                    return "This is in reference to a previous position.";

                case GameNotFoundReason.GameInactive:
                    return "This game is no longer in play.";

                default:
                    throw new NotImplementedException($"GameNotFoundReason.{reason} not translated.");
            }
        }

        public string DescribeStartTime(DateTime start)
        {
            if (start < DateTime.Now)
                return "right now";

            var delta = DateTime.Now.Subtract(start);

            if (delta.TotalMinutes < 5)
                return "very soon";

            if (delta.TotalMinutes < 60)
                return "within the hour";

            var hours = Math.Round(delta.TotalHours);
            if (hours == 1)
                return $"in about an hour";
            else
                return $"in about {Math.Round(delta.TotalHours)} hours";
        }

        public string Translate_to_DaysHours(TimeSpan? duration)
			=> duration != null
				? $"{((int)duration.Value.TotalDays)} days, {duration.Value.Hours} hours"
				: "no time";

		public string Translate_to_Hours(TimeSpan? duration)
			=> duration != null
				? $"{(Math.Round(duration.Value.TotalHours))} hours"
			: "no time";

        public string Translate_to_HoursAndMins(TimeSpan? duration)
            => duration != null
                ? $"{(Math.Floor(duration.Value.TotalHours))}h, {duration.Value.Minutes}m"
            : "no time";

    }
}

