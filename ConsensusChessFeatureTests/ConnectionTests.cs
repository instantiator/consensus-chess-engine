using System;
using System.Xml.Linq;
using ConsensusChessFeatureTests.Data;
using ConsensusChessFeatureTests.Database;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ConsensusChessFeatureTests
{
	[TestClass]
	public class ConnectionTests : AbstractFeatureTest
	{
		[TestMethod]
		public void CheckDatabaseConnection()
		{
            WriteLogLine($"Database: {Dbo.GetDb().DbPath}");

            using (var db = Dbo.GetDb())
            {
                var knownTables = new[]
                {
                    "board","commitment","games","media","move","participant","post","vote","node_state"
                };

                var dbTables = db.Model.GetEntityTypes()
                    .SelectMany(t => t.GetTableMappings())
                    .Select(m => m.Table.Name)
                    .Distinct()
                    .ToList();

                foreach (var table in knownTables)
                {
                    Assert.IsTrue(dbTables.Contains(table), $"Table {table} missing");
                    WriteLogLine($"Table ok: {table}");
                }
            }
        }

        [TestMethod]
        public async Task CanCreateNode()
        {
            if (File.Exists(AbstractConsensusService.HEALTHCHECK_READY_PATH))
                File.Delete(AbstractConsensusService.HEALTHCHECK_READY_PATH);

            var node = await StartNodeAsync();
            Assert.IsNotNull(node);
            Assert.IsTrue(File.Exists(AbstractConsensusService.HEALTHCHECK_READY_PATH));

            NodeSocialMock.Verify(ns => ns.StartListeningForCommandsAsync(
                It.IsAny<Func<SocialCommand, Task>>(),
                It.IsAny<bool>()),
                Times.Once);

            await node.StopAsync();
        }

        [TestMethod]
        public async Task CanCreateEngine()
        {
            if (File.Exists(AbstractConsensusService.HEALTHCHECK_READY_PATH))
                File.Delete(AbstractConsensusService.HEALTHCHECK_READY_PATH);

            var engine = await StartEngineAsync();
            Assert.IsNotNull(engine);
            Assert.IsTrue(File.Exists(AbstractConsensusService.HEALTHCHECK_READY_PATH));

            await engine.StopAsync();
        }
    }
}

