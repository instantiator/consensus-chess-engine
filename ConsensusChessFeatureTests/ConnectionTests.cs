using System;
using ConsensusChessFeatureTests.Data;
using ConsensusChessFeatureTests.Database;
using Microsoft.EntityFrameworkCore;

namespace ConsensusChessFeatureTests
{
	[TestClass]
	public class ConnectionTests : AbstractFeatureTest
	{
		[TestMethod]
		public void CheckDatabaseConnection()
		{
            using (var db = NodeDbo.GetDb())
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
                }
            }
        }
    }
}

