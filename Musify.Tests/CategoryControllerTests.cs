using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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

        [Test]
        public async Task GetAll_ShouldReturnOkWithList()
        {
            // Arrange done in Setup
            // Act
            var result = await _categoryController.GetAllCategories();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Should().NotBeNull();

            var payload = ok.Value.Should().BeAssignableTo<IEnumerable<Category>>().Subject;
            var list = payload as List<Category>;

            list.Should().NotBeNull();
            list.Should().HaveCount(5);

            list.Select(c => c.Name).Should().Contain(
            [
                "Drums and Percussion",
                "Guitars and Basses",
                "Acoustic Drumkits",
                "Snare Drums",
                "Brass Snare Drums"
            ]);

            list.Select(c => c.ParentId).Should().Contain(
            [
                null, null, 1, 1, 4
            ]);
        }

        [Test]
        public async Task GetById_WhenCategoryWithSpecifiedIdExists_ShouldReturnOk()
        {
            // Arrange
            // Acoustic Drumkits
            var existingCategoryId = 3;

            // Act
            var result = await _categoryController.GetCategoryById(existingCategoryId);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Should().NotBeNull();

            var category = ok.Value.Should().BeAssignableTo<Category>().Subject;

            category.Id.Should().Be(3);
            category.Name.Should().Be("Acoustic Drumkits");
            category.ParentId.Should().Be(1);
        }

        [Test]
        public async Task GetById_WhenCategoryWithSpecifiedIdDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistingCategoryId = 999;

            // Act
            var result = await _categoryController.GetCategoryById(nonExistingCategoryId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
