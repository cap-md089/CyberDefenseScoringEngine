#nullable disable

using Microsoft.EntityFrameworkCore;
using ScoringEngine.Models;

namespace ScoringEngine.Server.Data
{
    public class ScoringEngineDbContext : DbContext
    {
        public ScoringEngineDbContext(DbContextOptions<ScoringEngineDbContext> options)
            : base(options)
        {
        }

        public DbSet<ScoringItem> ScoringItems { get; set; }
        public DbSet<CompletedScoringItem> CompletedScoringItems { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<CompetitionSystem> CompetitionSystems { get; set; }
        public DbSet<RegisteredVirtualMachine> RegisteredVirtualMachines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScoringItem>(b =>
            {
                b.ToTable("ScoringItems");
            });

            modelBuilder.Entity<CompletedScoringItem>(b =>
            {
                b.ToTable("CompletedScoringItems");
            });

            modelBuilder.Entity<Team>(b =>
            {
                b.ToTable("Teams");
            });

            modelBuilder.Entity<CompetitionSystem>(b =>
            {
                b.ToTable("CompetitionSystems");
            });

            modelBuilder.Entity<RegisteredVirtualMachine>(b =>
            {
                b.ToTable("RegisteredVirtualMachine");
            });
        }
    }
}
