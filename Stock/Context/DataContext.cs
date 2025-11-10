using Microsoft.EntityFrameworkCore;
using Stock.Models;

namespace Stock.Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<Receive> Receives { get; set; } = null!;
        public DbSet<Issue> Issues { get; set; } = null !;
        public DbSet<StockWH> Stocks { get; set; } = null!;       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
