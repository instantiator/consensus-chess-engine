using System;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Helpers;
using ConsensusChessShared.Social;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ConsensusChessShared.Service
{
    public class CommandProcessor
    {
        public struct CommandRule
        {
            public CommandEnactionAsync Enaction;
            public bool RequireAuthorised;
            public bool MayRunRetrospectively;
        }

        public delegate Task CommandEnactionAsync(SocialCommand origin, IEnumerable<string> words);

        public event Func<SocialCommand, string, Task> OnFailAsync;

        private IEnumerable<string> skips;
        private IEnumerable<string> authorisedAccounts;
        private IDictionary<string, CommandRule> register;
        private ILogger log;

        public CommandProcessor(ILogger log, IEnumerable<string> authorisedAccounts, IEnumerable<string> skips)
        {
            this.log = log;
            this.authorisedAccounts = authorisedAccounts;
            this.skips = skips;
            register = new Dictionary<string, CommandRule>();
        }

        public void Register(string commandWord, bool requireAuthorised, bool runsRetrospectively, CommandEnactionAsync enaction)
        {
            register.Add(commandWord.ToLower(), new CommandRule()
            {
                Enaction = enaction,
                RequireAuthorised = requireAuthorised,
                MayRunRetrospectively = runsRetrospectively
            });
        }

        private bool IsAuthorised(string userId) { return authorisedAccounts.Contains(userId); }

		public async Task Parse(SocialCommand command)
		{
            log.LogTrace($"Command raw text: {command.RawText}");
            var commandWords = CommandHelper.ParseSocialCommand(command.RawText, skips);
            var commandWord = commandWords.FirstOrDefault()?.ToLower();

            try
            {
                if (string.IsNullOrWhiteSpace(commandWord))
                {
                    throw new CommandRejectionException(commandWords, command.NetworkUserId, CommandRejectionReason.NoCommandWords);
                }

                if (register.ContainsKey(commandWord))
                {
                    var rule = register[commandWord];
                    if (IsAuthorised(command.NetworkUserId) || !rule.RequireAuthorised)
                    {
                        if (!command.IsRetrospective || rule.MayRunRetrospectively)
                        {
                            log.LogInformation($"Executing command: {commandWord} from: {command.NetworkUserId}");
                            await rule.Enaction.Invoke(command, commandWords);
                        }
                        else
                        {
                            log.LogDebug($"Skipping retrospective command: {commandWord} from: {command.NetworkUserId}");
                        }
                    }
                    else
                    {
                        throw new CommandRejectionException(commandWords, command.NetworkUserId, CommandRejectionReason.NotAuthorised);
                    }
                }
                else
                {
                    throw new CommandRejectionException(commandWords, command.NetworkUserId, CommandRejectionReason.UnrecognisedCommand);
                }
            }
            catch (CommandRejectionException e)
            {
                log.LogWarning($"{e.Reason} (from {e.SenderId}): {string.Join(" ",e.Words)}");
                if (OnFailAsync != null)
                {
                    await OnFailAsync.Invoke(command, e.Reason.ToString());
                }
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unexpected exception parsing command: {string.Join(", ", commandWords)}");
                if (OnFailAsync != null)
                {
                    await OnFailAsync.Invoke(command, "Unexpected error.");
                }
            }
        }

    }
}

