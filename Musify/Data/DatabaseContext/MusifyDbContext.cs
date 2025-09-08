using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Musify.Models;

namespace Musify.Data.DatabaseContext
{
    public class MusifyDbContext : IdentityDbContext<IdentityUser>
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
    }
}
