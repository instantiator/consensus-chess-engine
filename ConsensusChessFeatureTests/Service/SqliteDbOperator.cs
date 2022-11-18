using System;
using ConsensusChessFeatureTests.Database;
using ConsensusChessShared.Database;
using ConsensusChessShared.Service;
using Microsoft.Extensions.Logging;

namespace ConsensusChessFeatureTests.Service
{
	public class SqliteDbOperator : DbOperator
	{
		private bool transient;

		public SqliteDbOperator(ILogger log, bool transient) : base(log, null)
		{
			this.transient = transient;
		}

        public override ConsensusChessSqliteContext GetDb()
			=> new ConsensusChessSqliteContext(transient ? GenerateFreshDbFilename() : null);

		private string GenerateFreshDbFilename()
		{
			var filename = $"{Guid.NewGuid()}.db";
			log.LogDebug($"Db filename: {filename}");
			return filename;
		}
    }
}
