using System;
using ConsensusChessShared.DTO;
using Microsoft.EntityFrameworkCore;

namespace ConsensusChessShared.Database
{
	public abstract class ConsensusChessDbContext : DbContext
	{
        public DbSet<Game> Games { get; set; }
        public DbSet<Participant> Participant { get; set; }
        public DbSet<NodeState> NodeState { get; set; }

        public ConsensusChessDbContext() : base()
        {
        }

        protected abstract override void OnConfiguring(DbContextOptionsBuilder optionsBuilder);
    }
}

