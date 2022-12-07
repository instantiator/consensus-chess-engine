using System;
using ConsensusChessShared.DTO;
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

        public struct UnrecognisedCommandRule
        {
            public UnrecognisedCommandEnactionAsync Enaction;
            public bool RequireAuthorised;
            public bool MayRunRetrospectively;
        }

        /// <summary>
        /// Executes a specific command.
        /// </summary>
        /// <param name="origin">originating social message</param>
        /// <param name="words">parameters extracted from the message (split on space)</param>
        /// <exception cref="CommandRejectionException">An issue occurred whilst executing the command</throws>
        public delegate Task CommandEnactionAsync(SocialCommand origin, IEnumerable<string> words);

        /// <summary>
        /// Accepts (and optionally executes) an arbitrary command, indicating whether it attempted to do so.
        /// </summary>
        /// <param name="origin">originating social message</param>
        /// <returns>True if the command execution was attempted (and succeeded), false if it was not attempted</returns>
        /// <exception cref="CommandRejectionException">An issue occurred whilst executing the command</throws>
        public delegate Task<bool> UnrecognisedCommandEnactionAsync(SocialCommand origin);

        public event Func<SocialCommand, string, CommandRejectionReason, Task>? OnFailAsync;

        private IEnumerable<string> skips;
        private IEnumerable<string> authorisedAccounts;
        private IDictionary<string, CommandRule> register;
        private List<UnrecognisedCommandRule> unregister;
        private List<string> ignorables;
        private ILogger log;
        private SocialUsername self;

        public CommandProcessor(ILogger log, IEnumerable<string> authorisedAccounts, SocialUsername self, IEnumerable<string> skips, IEnumerable<string> ignorables)
        {
            this.log = log;
            this.authorisedAccounts = authorisedAccounts;
            this.skips = skips;
            this.self = self;
            this.register = new Dictionary<string, CommandRule>();
            this.unregister = new List<UnrecognisedCommandRule>();
            this.ignorables = new List<string>(ignorables);
        }

        /// <summary>
        /// Register a <see cref="CommandEnactionAsync"/> for exection.
        /// </summary>
        /// <param name="commandWord">keyword to trigger the command</param>
        /// <param name="requireAuthorised">set True if only authorised users may invoke this command</param>
        /// <param name="runsRetrospectively">set True to run this if it was found to have been sent before the service most recently started</param>
        /// <param name="enaction"><see cref="CommandEnactionAsync"/> to invoke when the command is triggered</param>
        public void Register(string commandWord, bool requireAuthorised, bool runsRetrospectively, CommandEnactionAsync enaction)
        {
            register.Add(commandWord.ToLower(), new CommandRule()
            {
                Enaction = enaction,
                RequireAuthorised = requireAuthorised,
                MayRunRetrospectively = runsRetrospectively
            });
        }

        /// <summary>
        /// Register a <see cref="UnrecognisedCommandEnactionAsync"/> for exection.
        /// </summary>
        /// <param name="requireAuthorised">set True if only commands from authorised users should be passed through</param>
        /// <param name="runsRetrospectively">set True to run this if it was found to have been sent before the service most recently started</param>
        /// <param name="enaction"><see cref="CommandEnactionAsync"/> to invoke</param>
        public void RegisterUnrecognised(bool requireAuthorised, bool runsRetrospectively, UnrecognisedCommandEnactionAsync enaction)
        {
            unregister.Add(new UnrecognisedCommandRule()
            {
                Enaction = enaction,
                RequireAuthorised = requireAuthorised,
                MayRunRetrospectively = runsRetrospectively
            });
        }

        /// <summary>
        /// Parse a command delivered from a social network. See also: <seealso cref="SocialCommand"/>
        /// </summary>
        /// <param name="command">The <see cref="SocialCommand"/> to parse</param>
		public async Task ParseAsync(SocialCommand command)
		{
            if (self.Equals(command.SourceUsername))
            {
                log.LogDebug("Electing not to parse own message.");
                return;
            }

            log.LogDebug($"Parsing new command from: {command.SourceUsername.Full}");
            log.LogTrace($"Command raw text: {command.RawText}");

            var commandWords = CommandHelper.ParseSocialCommand(command.RawText!, skips);

            log.LogDebug($"Words: {string.Join(" ", commandWords)}");

            if (commandWords.Any(w => ignorables.Any(iw => iw.ToLower() == w.ToLower())))
            {
                log.LogDebug("Command ignored.");
                return;
            }

            var commandWord = commandWords.FirstOrDefault()?.ToLower();

            try
            {
                if (string.IsNullOrWhiteSpace(commandWord))
                {
                    var accepted = await ParseUnrecognisedCommandAsync(command);
                    if (accepted == 0)
                    {
                        throw new CommandRejectionException(
                            command,
                            commandWords,
                            CommandRejectionReason.NoCommandWords);
                    }
                    else
                    {
                        return; // no need to continue: commandWord was blank
                    }
                }

                if (register.ContainsKey(commandWord))
                {
                    var rule = register[commandWord];

                    // check authorised
                    if (!rule.RequireAuthorised || command.IsAuthorised)
                    {
                        // check retrospective
                        if (!command.IsRetrospective || rule.MayRunRetrospectively)
                        {
                            log.LogInformation($"Executing command: {commandWord} from: {command.SourceUsername.Full}");
                            await rule.Enaction.Invoke(command, commandWords);
                        }
                        else
                        {
                            log.LogDebug($"Rejecting retrospective command: {commandWord} from: {command.SourceUsername.Full}");
                            throw new CommandRejectionException(
                                command,
                                commandWords,
                                CommandRejectionReason.NotForRetrospectiveExecution);
                        }
                    }
                    else
                    {
                        log.LogDebug($"Rejecting unauthorised command: {commandWord} from: {command.SourceUsername.Full}");
                        throw new CommandRejectionException(
                            command,
                            commandWords,
                            CommandRejectionReason.NotAuthorised);
                    }
                }
                else
                {
                    // command word not recognised
                    var accepted = await ParseUnrecognisedCommandAsync(command);
                    if (accepted == 0)
                    {
                        log.LogDebug($"Rejecting unrecognised command: {commandWord} from: {command.SourceUsername.Full}");
                        throw new CommandRejectionException(
                            command,
                            commandWords,
                            CommandRejectionReason.UnrecognisedCommand);
                    }
                }
            }
            catch (CommandRejectionException e)
            {
                log.LogWarning($"{e.Reason} (from {e.Command.SourceUsername.Full}): {string.Join(" ",e.Words)}");
                if (OnFailAsync != null)
                {
                    await OnFailAsync.Invoke(e.Command, e.Reason.ToString(), e.Reason);
                }
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unexpected exception parsing command: {string.Join(", ", commandWords)}");
                if (OnFailAsync != null)
                {
                    await OnFailAsync.Invoke(command, "Unexpected error.", CommandRejectionReason.UnexpectedException);
                }
            }
        }

        private async Task<int> ParseUnrecognisedCommandAsync(SocialCommand command)
        {
            var accepted = 0;
            foreach (var executor in unregister)
            {
                try
                {
                    var ok = await executor.Enaction.Invoke(command);
                    if (ok)
                    {
                        log.LogInformation($"Command executed: {command.RawText} from: {command.SourceUsername.Full}");
                        accepted++;
                    }
                }
                catch (Exception e)
                {
                    log.LogError(e, $"Unexpected exception parsing command: {command.RawText}");
                }
            }
            return accepted;
        }

    }
}

