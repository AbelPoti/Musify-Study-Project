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

        // This method is called even when using AddDbContext in Program.cs, being ideal for context configurations
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.LogTo(Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your entity mappings here
            // Example: modelBuilder.Entity<YourEntity>().ToTable("YourTableName");
            base.OnModelCreating(modelBuilder);

            // To prevent multiple cascade paths issue
            modelBuilder.Entity<Instrument>()
                .HasMany(i => i.Attributes)
                .WithOne(av => av.Instrument)
                .HasForeignKey(av => av.InstrumentId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<AttributeDefinition>()
                .Property(a => a.DataType)
                .HasConversion<string>();
        }

        public DbSet<Instrument> Instruments { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<ShopItem> ShopItems { get; set; }

        public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }

        public DbSet<InstrumentAttributeValue> InstrumentAttributeValues { get; set; }
    }
}
