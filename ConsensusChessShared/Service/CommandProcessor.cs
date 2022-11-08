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
            public CommandEnaction Enaction;
            public bool RequireAuthorised;
        }

        public delegate Task CommandEnaction(IEnumerable<string> words);

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

        public void Register(string commandWord, bool requireAuthorised, CommandEnaction enaction)
        {
            register.Add(commandWord.ToLower(), new CommandRule()
            {
                Enaction = enaction,
                RequireAuthorised = requireAuthorised
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
                        log.LogInformation($"{command.NetworkUserId} issued command: {commandWord}");
                        await rule.Enaction.Invoke(commandWords);
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
                // TODO: reply with refusal
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unexpected exception parsing command: {string.Join(", ", commandWords)}");
                // TODO: reply with refusal
            }
        }

    }
}

