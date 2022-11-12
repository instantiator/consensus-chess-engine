using System.Collections;
using ConsensusChessShared.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ConsensusChessShared.Database
{
	public class ConsensusChessDbContext : DbContext
	{
        public DbSet<Game> Games { get; set; }
        public DbSet<NodeState> NodeStates { get; set; }

        private string host;
        private string database;
        private string username;
        private string password;

        public ConsensusChessDbContext() { } // design time constructor

        public ConsensusChessDbContext(string host, string database, string username, string password)
        {
            this.host = host;
            this.database = database;
            this.username = username;
            this.password = password;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .ConfigureWarnings((wcb) => wcb.Log(CoreEventId.LazyLoadOnDisposedContextWarning))
            .UseLazyLoadingProxies()
            .UseNpgsql($"Host={host};Database={database};Username={username};Password={password};Include Error Detail=true")
            .UseSnakeCaseNamingConvention();

        public static ConsensusChessDbContext FromEnvironment(System.Collections.IDictionary env)
        {
            var environment = env.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);
            var host = environment["DB_HOST"];
            var database = environment["POSTGRES_DB"];
            var username = environment["POSTGRES_USER"];
            var password = environment["POSTGRES_PASSWORD"];
            return new ConsensusChessDbContext(host, database, username, password);
        }
    }
}

