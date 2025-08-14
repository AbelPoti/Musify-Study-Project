using Microsoft.EntityFrameworkCore;
using Musify.Models;

namespace Musify.Data.DatabaseContext
{
    public class MusifyDbContext : DbContext
    {
        public MusifyDbContext(DbContextOptions<MusifyDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your entity mappings here
            // Example: modelBuilder.Entity<YourEntity>().ToTable("YourTableName");
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Instrument> Instruments { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<ShopItem> ShopItems { get; set; }

        public DbSet<User> Users { get; set; }
    }
}
