using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Controllers;
using Musify.Data.DatabaseContext;
using Musify.Dtos.AttributeDefinitionDtos;
using Musify.Models;

namespace Musify.Tests.ControllerUnitTests
{
    [TestFixture]
    internal class AttributeDefinitionControllerTests
    {
        private MusifyDbContext _dbContext;
        private AttributeDefinitionController _attributeDefinitionController;

        [SetUp]
        public void Setup()
        {
            // Use a unique name per test class to isolate data between tests
            var options = new DbContextOptionsBuilder<MusifyDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new MusifyDbContext(options);

            SeedDatabase();

            _attributeDefinitionController = new AttributeDefinitionController(_dbContext);
        }

        private void SeedDatabase()
        {
            _dbContext.Database.EnsureCreated();

            // Seed Categories (parent entities)
            _dbContext.Categories.AddRange(
                new Category { Id = 1, Name = "Drums and Percussion", ParentId = null },
                new Category { Id = 2, Name = "Snare Drums", ParentId = 1 },
                new Category { Id = 3, Name = "Brass Snare Drums", ParentId = 2 }
            );

            // Seed AttributeDefinitions
            _dbContext.AttributeDefinitions.AddRange(
                new AttributeDefinition
                {
                    Id = 1,
                    Name = "Diameter",
                    DataType = AttributeDefinitionDataType.Decimal,
                    CategoryId = 2,
                    Category = _dbContext.Categories.Find(2)!
                },
                new AttributeDefinition
                {
                    Id = 2,
                    Name = "Brass Alloy",
                    DataType = AttributeDefinitionDataType.String,
                    CategoryId = 3,
                    Category = _dbContext.Categories.Find(3)!
                }
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
            var result = await _attributeDefinitionController.GetAllAttributeDefinitions();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeAssignableTo<IEnumerable<AttributeDefinitionReadDetailedDto>>().Subject;

            var list = payload as List<AttributeDefinitionReadDetailedDto>;

            list.Should().NotBeNull();
            list.Count.Should().Be(2);

            list.Select(ad => ad.Name).Should().Contain(["Diameter", "Brass Alloy"]);
            list.Select(ad => ad.CategoryId).Should().Contain([2, 3]);
            list.Select(ad => ad.Category).Should().Contain(
            [
                (await _dbContext.Categories.FindAsync(2))!,
                (await _dbContext.Categories.FindAsync(3))!
            ]);
        }

        [Test]
        public async Task GetById_WhenAttributeDefinitionWithProvidedIdExists_ShouldReturnOk()
        {
            // Arrange
            const int existingId = 1;

            // Act
            var result = await _attributeDefinitionController.GetAttributeDefinitionById(existingId);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var attributeDefinition = ok.Value.Should().BeAssignableTo<AttributeDefinitionReadDetailedDto>().Subject;

            attributeDefinition.Id.Should().Be(1);
            attributeDefinition.Name.Should().Be("Diameter");
            attributeDefinition.DataType.Should().Be(AttributeDefinitionDataType.Decimal);
            attributeDefinition.CategoryId.Should().Be(2);
            attributeDefinition.Category.Should().Be(await _dbContext.Categories.FindAsync(2));
        }

        [Test]
        public async Task GetById_WhenNoAttributeDefinitionWithProvidedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int nonexistentId = 999;

            // Act
            var result = await _attributeDefinitionController.GetAttributeDefinitionById(nonexistentId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task GetByCategoryId_WhenCategoryWithProvidedCategoryIdExists_ShouldReturnOkWithList()
        {
            // Arrange
            const int existingCategoryId = 2;

            // Act
            var result = await _attributeDefinitionController.GetAttributeDefinitionsByCategoryId(existingCategoryId);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeAssignableTo<IEnumerable<AttributeDefinitionReadDetailedDto>>().Subject;

            var list = payload as List<AttributeDefinitionReadDetailedDto>;
            list.Should().NotBeNull();
            list.Should().HaveCount(1);

            var attributeDefinition = list.First();
            attributeDefinition.Id.Should().Be(1);
            attributeDefinition.Name.Should().Be("Diameter");
            attributeDefinition.DataType.Should().Be(AttributeDefinitionDataType.Decimal);
            attributeDefinition.CategoryId.Should().Be(2);
            attributeDefinition.Category.Should().Be(await _dbContext.Categories.FindAsync(2));
        }

        [Test]
        public async Task GetByCategoryId_WhenNoCategoryWithProvidedCategoryIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int nonexistentCategoryId = 999;

            // Act
            var result =
                await _attributeDefinitionController.GetAttributeDefinitionsByCategoryId(nonexistentCategoryId);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<AttributeDefinitionGetByCategoryIdNotFoundResponseDto>().Subject;
            payload.Message.Should().Be("Category not found.");

        }

        [Test]
        public async Task Create_WhenProvidedDataIsValid_ShouldReturnCreated()
        {
            // Arrange
            var dto = new AttributeDefinitionCreateDto
            {
                Name = "Material",
                DataType = AttributeDefinitionDataType.String,
                CategoryId = 2
            };

            // Act
            var result = await _attributeDefinitionController.CreateAttributeDefinition(dto);

            // Assert
            var createdAt = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAt.Should().NotBeNull();

            createdAt.RouteValues.Should().NotBeNull();
            createdAt.RouteValues.Keys.Should().Contain("id");
            createdAt.RouteValues["id"].Should().Be(3);

            var createdAttributeDefinition = createdAt.Value.Should().BeAssignableTo<AttributeDefinitionReadDetailedDto>().Subject;
            createdAttributeDefinition.Id.Should().Be(3);
            createdAttributeDefinition.Name.Should().Be("Material");
            createdAttributeDefinition.DataType.Should().Be(AttributeDefinitionDataType.String);
            createdAttributeDefinition.Category.Should().Be(await _dbContext.Categories.FindAsync(dto.CategoryId));
        }

        [Test]
        public async Task Create_WhenProvidedCategoryDoesNotExist_ShouldReturnBadRequest()
        {
            // Arrange
            var dto = new AttributeDefinitionCreateDto
            {
                Name = "Material",
                DataType = AttributeDefinitionDataType.String,
                CategoryId = 999 // Does not exist
            };

            // Act
            var result = await _attributeDefinitionController.CreateAttributeDefinition(dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<AttributeDefinitionCreateBadRequestResponseDto>().Subject;

            payload.Message.Should().Be("Associated category does not exist.");
        }

        [Test]
        public async Task Update_WhenProvidedDataIsValid_ShouldReturnNoContent()
        {
            // Arrange
            const int id = 1;

            var dto = new AttributeDefinitionUpdateDto
            {
                Id = id,
                Name = "Drums and Percussion Updated",
                DataType = AttributeDefinitionDataType.String,
                CategoryId = 2
            };

            // Act
            var result = await _attributeDefinitionController.UpdateAttributeDefinition(id, dto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task Update_WhenIdsProvidedInPathAndBodyDoNotMatch_ShouldReturnBadRequest()
        {
            // Arrange
            const int id = 1;

            var dto = new AttributeDefinitionUpdateDto
            {
                Id = id + 1,
                Name = "Drums and Percussion Updated",
                DataType = AttributeDefinitionDataType.String,
                CategoryId = 2
            };

            // Act
            var result = await _attributeDefinitionController.UpdateAttributeDefinition(id, dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<AttributeDefinitionUpdateBadRequestResponseDto>().Subject;

            payload.Message.Should().Be("ID in URL does not match ID in body.");
        }

        [Test]
        public async Task Update_WhenAttributeDefinitionDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            const int nonexistentId = 999;
            var dto = new AttributeDefinitionUpdateDto
            {
                Id = nonexistentId,
                Name = "Nonexistent",
                DataType = AttributeDefinitionDataType.String,
                CategoryId = 2
            };

            // Act
            var result = await _attributeDefinitionController.UpdateAttributeDefinition(nonexistentId, dto);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<AttributeDefinitionUpdateNotFoundResponseDto>().Subject;
            payload.Message.Should().Be("Attribute definition not found.");
        }

        [Test]
        public async Task Update_WhenProvidedCategoryDoesNotExist_ShouldReturnBadRequest()
        {
            // Arrange
            const int id = 1;
            var dto = new AttributeDefinitionUpdateDto
            {
                Id = id,
                Name = "Drums and Percussion Updated",
                DataType = AttributeDefinitionDataType.String,
                CategoryId = 999 // Does not exist
            };

            // Act
            var result = await _attributeDefinitionController.UpdateAttributeDefinition(id, dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<AttributeDefinitionUpdateBadRequestResponseDto>().Subject;
            payload.Message.Should().Be("Associated category does not exist.");
        }

        [Test]
        public async Task Delete_WhenAttributeDefinitionWithProvidedIdExists_ShouldReturnNoContent()
        {
            // Arrange
            const int existingId = 1;

            // Act
            var result = await _attributeDefinitionController.DeleteAttributeDefinition(existingId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task Delete_WhenAttributeDefinitionWithProvidedIdDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            const int nonexistentId = 999;

            // Act
            var result = await _attributeDefinitionController.DeleteAttributeDefinition(nonexistentId);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<AttributeDefinitionDeleteNotFoundResponseDto>().Subject;
            payload.Message.Should().Be("Attribute definition not found.");
        }
    }
}
