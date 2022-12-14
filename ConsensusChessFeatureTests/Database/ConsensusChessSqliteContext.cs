using System;
using ConsensusChessShared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ConsensusChessFeatureTests.Database
{
    public class ConsensusChessSqliteContext : ConsensusChessDbContext
    {
        public string DbPath { get; private set; }

        // design time constructor
        public ConsensusChessSqliteContext() : base() { }

        public ConsensusChessSqliteContext(string? prefix = "feature-tests", DateTime? started = null) : base()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, $"{prefix}.{started?.Ticks.ToString() ?? "-"}.consensus.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .ConfigureWarnings((wcb) => wcb.Log(CoreEventId.LazyLoadOnDisposedContextWarning))
                .UseLazyLoadingProxies()
                .UseSqlite($"Data Source={DbPath!}")
                .UseSnakeCaseNamingConvention();
    }
}
