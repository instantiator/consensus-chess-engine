using System;
using ConsensusChessShared.DTO;
using Microsoft.EntityFrameworkCore;

namespace ConsensusChessShared.Database
{
	public class ConsensusChessDbContext : DbContext
	{
        public DbSet<Game> Games { get; set; }
        public DbSet<Node> Nodes { get; set; }

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
            var host = (string)env["DB_HOST"];
            var database = (string)env["POSTGRES_DB"];
            var username = (string)env["POSTGRES_USER"];
            var password = (string)env["POSTGRES_PASSWORD"];
            return new ConsensusChessDbContext(host, database, username, password);
        }
    }
}

