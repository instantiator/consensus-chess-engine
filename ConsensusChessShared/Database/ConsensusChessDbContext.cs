using System;
using System.Collections;
using System.Linq;
using ConsensusChessShared.DTO;
using Microsoft.EntityFrameworkCore;

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
            => optionsBuilder.UseNpgsql($"Host={host};Database={database};Username={username};Password={password}");

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

