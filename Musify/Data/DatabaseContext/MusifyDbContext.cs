using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Musify.Models;

namespace Musify.Data.DatabaseContext
{
    public class MusifyDbContext : IdentityDbContext<ApplicationUser>
    {
        public MusifyDbContext(DbContextOptions<MusifyDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your entity mappings here
            // Example: modelBuilder.Entity<YourEntity>().ToTable("YourTableName");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Instrument>()
                .HasMany(i => i.CustomAttributes)
                .WithOne(av => av.Instrument)
                .HasForeignKey(av => av.InstrumentId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }

        public DbSet<Instrument> Instruments { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<ShopItem> ShopItems { get; set; }

        public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }

        public DbSet<InstrumentAttributeValue> InstrumentAttributeValues { get; set; }
    }
}
