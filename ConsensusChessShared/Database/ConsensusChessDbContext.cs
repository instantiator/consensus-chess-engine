using System;
using ConsensusChessShared.DTO;
using Microsoft.EntityFrameworkCore;

namespace ConsensusChessShared.Database
{
	public class ConsensusChessDbContext : DbContext
	{
        public DbSet<Game> Games { get; set; }

        //public DbSet<Board> Boards { get; set; }
        //public DbSet<Commitment> Commitments { get; set; } 
        //public DbSet<Move> Moves { get; set; }
        //public DbSet<Network> Networks { get; set; }
        //public DbSet<Participant> Participants { get; set; }
        //public DbSet<Post> Posts { get; set; }
        //public DbSet<Vote> Votes { get; set; }
        //public DbSet<VoteValidation> VoteValidations { get; set; }

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

    }
}

