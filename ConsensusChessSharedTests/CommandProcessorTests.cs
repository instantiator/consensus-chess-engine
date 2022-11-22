using System;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using ConsensusChessSharedTests.Data;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConsensusChessSharedTests
{
    [TestClass]
    public class CommandProcessorTests
    {
        private Mock<ILogger> mockLogger;
        private IEnumerable<string> authorisedAccounts;
        private IEnumerable<string> skips;
        private CommandProcessor cmd;
        private Network fakeNetwork;

        private List<Tuple<SocialCommand, IEnumerable<string>>> enactions;
        private List<Tuple<SocialCommand, string, CommandRejectionReason?>> fails;

        [TestInitialize]
        public void Init()
        {
            mockLogger = new Mock<ILogger>();
            authorisedAccounts = SampleDataGenerator.AuthorisedAccounts;
            skips = SampleDataGenerator.Skips;
            fakeNetwork = SampleDataGenerator.FakeNetwork;
            cmd = new CommandProcessor(mockLogger.Object, authorisedAccounts, skips);
            cmd.OnFailAsync += Cmd_OnFailAsync;
            enactions = new List<Tuple<SocialCommand, IEnumerable<string>>>();
            fails = new List<Tuple<SocialCommand, string, CommandRejectionReason?>>();
        }

        private async Task Cmd_OnFailAsync(SocialCommand origin, string error, CommandRejectionReason reason)
        {
            fails.Add(new Tuple<SocialCommand, string, CommandRejectionReason?>(origin, error, reason));
        }

        private async Task EnactionAsync(SocialCommand origin, IEnumerable<string> words)
        {
            enactions.Add(new Tuple<SocialCommand, IEnumerable<string>>(origin, words));
        }

        [TestMethod]
        public async Task Register_registers_Command()
        {
            cmd.Register("keyword", requireAuthorised: false, runsRetrospectively: true, EnactionAsync);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(0, fails.Count());

            var badCommand = SampleDataGenerator.SimpleCommand("nope not the right word");
            await cmd.ParseAsync(badCommand);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(1, fails.Count());

            var goodCommand = SampleDataGenerator.SimpleCommand("keyword and some other words");
            await cmd.ParseAsync(goodCommand);
            Assert.AreEqual(1, enactions.Count());
            Assert.AreEqual(1, fails.Count());
        }

        [TestMethod]
        public async Task Parse_skips_SkipWords()
        {
            cmd.Register("keyword", requireAuthorised: false, runsRetrospectively: true, EnactionAsync);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(0, fails.Count());

            var command = SampleDataGenerator.SimpleCommand("@icgames keyword thirdWord");
            await cmd.ParseAsync(command);
            Assert.AreEqual(1, enactions.Count());
            var enactedWords = enactions.First().Item2;
            Assert.AreEqual(2, enactedWords.Count());
            Assert.AreEqual("keyword", enactedWords.ElementAt(0));
            Assert.AreEqual("thirdWord", enactedWords.ElementAt(1));
        }

        [TestMethod]
        public async Task Parse_canReject_RetrospectiveCommands()
        {
            cmd.Register("keyword_no_retro", requireAuthorised: false, runsRetrospectively: false, EnactionAsync);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(0, fails.Count());

            var command_retro = SampleDataGenerator.SimpleCommand("keyword_no_retro", isRetrospective: true);
            await cmd.ParseAsync(command_retro);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(1, fails.Count());

            var command_stream = SampleDataGenerator.SimpleCommand("keyword_no_retro", isRetrospective: false);
            await cmd.ParseAsync(command_stream);
            Assert.AreEqual(1, enactions.Count());
            Assert.AreEqual(1, fails.Count());
        }

        [TestMethod]
        public async Task Parse_canReject_UnauthorisedCommands()
        {
            cmd.Register("drop_table_CrownJewels", requireAuthorised: true, runsRetrospectively: true, EnactionAsync);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(0, fails.Count());

            var command_unauthorised = SampleDataGenerator.SimpleCommand("drop_table_CrownJewels", isAuthorised: false);
            await cmd.ParseAsync(command_unauthorised);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(1, fails.Count());

            var command_authorised = SampleDataGenerator.SimpleCommand("drop_table_CrownJewels", isAuthorised: true);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(1, fails.Count());
        }

        [TestMethod]
        public async Task Parse_rejects_EmptyCommands()
        {
            cmd.Register("keyword", requireAuthorised: false, runsRetrospectively: true, EnactionAsync);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(0, fails.Count());

            var command_empty = SampleDataGenerator.SimpleCommand("", isAuthorised: false);
            await cmd.ParseAsync(command_empty);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(1, fails.Count());

            var command_whitespace = SampleDataGenerator.SimpleCommand("  ", isAuthorised: false);
            await cmd.ParseAsync(command_whitespace);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(2, fails.Count());

            var command_justSkip = SampleDataGenerator.SimpleCommand("@icgames", isAuthorised: false);
            await cmd.ParseAsync(command_justSkip);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(3, fails.Count());

            var command_justSkip_whitespace = SampleDataGenerator.SimpleCommand("@icgames   ", isAuthorised: false);
            await cmd.ParseAsync(command_justSkip_whitespace);
            Assert.AreEqual(0, enactions.Count());
            Assert.AreEqual(4, fails.Count());
        }
    }
}
