using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Controllers;
using Musify.Models;
using Musify.Data.DatabaseContext;
using Musify.Dtos.CategoryDtos;

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
            var existingCategoryId = 3; // Acoustic Drumkits

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

        [Test]
        public async Task Create_WhenProvidedParentCategoryIdExists_ShouldReturnCreated()
        {
            var dto = new CategoryCreateDto
            {
                Name = "New Category",
                ParentId = 1 // Drums and Percussion
            };

            // Act
            var result = await _categoryController.CreateCategory(dto);

            // Assert
            var createdAt = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAt.Should().NotBeNull();

            createdAt.RouteValues.Should().NotBeNull();
            createdAt.RouteValues.Keys.Should().Contain("id");
            createdAt.RouteValues["id"].Should().Be(6);

            var createdCategory = createdAt.Value.Should().BeAssignableTo<Category>().Subject;
            createdCategory.Name.Should().Be("New Category");
            createdCategory.ParentId.Should().Be(1);
        }

        [Test]
        public async Task Create_WhenProvidedParentCategoryIdDoesNotExist_ShouldReturnBadRequest()
        {
            var dto = new CategoryCreateDto
            {
                Name = "Invalid Category",
                ParentId = 999 // Non-existing parent
            };

            // Act
            var result = await _categoryController.CreateCategory(dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Should().NotBeNull();

            var payload = badRequest.Value.Should().BeOfType<CategoryCreateBadRequestResponseDto>().Subject;
            payload.Message.Should().Be("Parent category does not exist.");
        }
    }
}
