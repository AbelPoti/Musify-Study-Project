using Microsoft.EntityFrameworkCore;
using Musify.Controllers;
using Musify.Models;
using Musify.Data.DatabaseContext;

namespace Musify.Tests
{
    [TestFixture]
    public class CategoryControllerTests
    {
        private MusifyDbContext _dbContext;
        private CategoryController _categoryController;

        [SetUp]
        public void Setup()
        {
            // Use a unique name per test class (or test) to isolate data between tests
            var options = new DbContextOptionsBuilder<MusifyDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new MusifyDbContext(options);

            SeedDatabase();

            _categoryController = new CategoryController(_dbContext);
        }

        public void SeedDatabase()
        {
            _dbContext.Categories.AddRange(
                new Category { Id = 1, Name = "Drums and Percussion", ParentId = null },
                new Category { Id = 2, Name = "Guitars and Basses", ParentId = null },
                new Category { Id = 3, Name = "Acoustic Drumkits", ParentId = 1 },
                new Category { Id = 4, Name = "Snare Drums", ParentId = 1 },
                new Category { Id = 5, Name = "Brass Snare Drums", ParentId = 4 }
            );

            _dbContext.SaveChanges();
        }


        [TearDown]
        public void Teardown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
