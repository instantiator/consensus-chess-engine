using System;
using ConsensusChessFeatureTests.Database;
using ConsensusChessShared.Database;
using ConsensusChessShared.Service;
using Microsoft.Extensions.Logging;

namespace ConsensusChessFeatureTests.Service
{
	public class SqliteDbOperator : DbOperator
	{
		private string? test;
		private DateTime? started;

		public SqliteDbOperator(ILogger log, string? test = null, DateTime? started = null) : base(log, null)
		{
			this.test = test;
			this.started = started;
		}

        public override ConsensusChessSqliteContext GetDb()
			=> new ConsensusChessSqliteContext(test, started);

		[Obsolete("Name database by test")]
		private string GenerateFreshDbFilename()
		{
			var filename = $"{Guid.NewGuid()}.db";
			log.LogDebug($"Db filename: {filename}");
			return filename;
		}
    }
}
