using System.Collections;
using ConsensusChessShared.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ConsensusChessShared.Database
{
	public class ConsensusChessPostgresContext : ConsensusChessDbContext
	{
        private string host;
        private string database;
        private string username;
        private string password;
        private int port;

        // design time
        public ConsensusChessPostgresContext() : base()
        {
        }

        public ConsensusChessPostgresContext(string host, string database, string username, string password, int port) : base()
        {
            this.host = host;
            this.database = database;
            this.username = username;
            this.password = password;
            this.port = port;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .ConfigureWarnings((wcb) => wcb.Log(CoreEventId.LazyLoadOnDisposedContextWarning))
            .UseLazyLoadingProxies()
            .UseNpgsql($"Host={host};Database={database};Username={username};Password={password};Port={port};Include Error Detail=true")
            .UseSnakeCaseNamingConvention();

        public static ConsensusChessPostgresContext FromEnv(IDictionary env)
        {
            var environment = env is Dictionary<string, string>
                ? env as Dictionary<string, string>
                : env.Cast<DictionaryEntry>().ToDictionary(x => (string)x.Key, x => (string)x.Value!);

            var host = environment["DB_HOST"];
            var database = environment["POSTGRES_DB"];
            var username = environment["POSTGRES_USER"];
            var password = environment["POSTGRES_PASSWORD"];
            var port = int.Parse(environment["DB_PORT"]);
            return new ConsensusChessPostgresContext(host, database, username, password, port);
        }
    }
}

