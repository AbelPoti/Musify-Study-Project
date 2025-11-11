using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Controllers;
using Musify.Data.DatabaseContext;
using Musify.Dtos.AttributeValueDtos;
using Musify.Dtos.InstrumentDtos;
using Musify.Models;

namespace Musify.Tests.ControllerUnitTests
{
    [TestFixture]
    internal class InstrumentControllerTests
    {
        private MusifyDbContext _dbContext;
        private InstrumentController _instrumentController;

        [SetUp]
        public void Setup()
        {
            // Use a unique name per test class to isolate data between tests
            var options = new DbContextOptionsBuilder<MusifyDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new MusifyDbContext(options);

            SeedDatabase();

            _instrumentController = new InstrumentController(_dbContext);
        }

        public void SeedDatabase()
        {
            _dbContext.Database.EnsureCreated();

            // Seed Categories (parent entities)
            _dbContext.Categories.AddRange(
                new Category { Id = 1, Name = "Drums and Percussion", ParentId = null },
                new Category { Id = 2, Name = "Snare Drums", ParentId = 1 },
                new Category { Id = 3, Name = "Brass Snare Drums", ParentId = 2 },
                new Category { Id = 4, Name = "Acoustic Drum Sets", ParentId = 1}   
            );

            // Seed instruments
            _dbContext.Instruments.AddRange(
                new Instrument
                {
                    Id = 1,
                    Name = "Dialtune 14\"x6.5\" Black Nickel Brass SD",
                    Brand = "Dialtune",
                    CategoryId = 3,
                    Category = _dbContext.Categories.Find(3)!,
                    Description = null,
                    Attributes = []
                },
                new Instrument
                {
                    Id = 2,
                    Name = "Sonor 14\"x6.5\" Chrome over Brass Sn.",
                    Brand = "Sonor",
                    CategoryId = 3,
                    Category = _dbContext.Categories.Find(3)!,
                    Description = "A cool snare drum with a less cool description",
                    Attributes = []
                },
                new Instrument
                {
                    Id = 3,
                    Name = "Yamaha Stage Custom Birch 5-Piece Drum Set",
                    Brand = "Yamaha",
                    CategoryId = 4,
                    Category = _dbContext.Categories.Find(4)!,
                    Description = "A great intermediate drum set",
                    Attributes = []
                }
            );

            _dbContext.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetAll_ShouldReturnOkWithList()
        {
            // Arrange done in Setup
            // Act
            var result = await _instrumentController.GetAllInstruments();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeAssignableTo<IEnumerable<InstrumentReadMinimalDto>>().Subject;

            var list = payload as List<InstrumentReadMinimalDto>;

            list.Should().NotBeNull();
            list.Count.Should().Be(3);

            list.Select(i => i.Name).Should().Contain(
            [
                "Dialtune 14\"x6.5\" Black Nickel Brass SD",
                "Sonor 14\"x6.5\" Chrome over Brass Sn.",
                "Yamaha Stage Custom Birch 5-Piece Drum Set"
            ]);

            list.Select(i => i.Brand).Should().Contain(
            [
                "Dialtune", "Sonor", "Yamaha"
            ]);

            list.Select(i => i.CategoryId).Should().Contain([3, 3, 4]);

            list.Select(i => i.Description).Should().Contain(
            [
                null,
                "A cool snare drum with a less cool description",
                "A great intermediate drum set"
            ]);
        }

        [Test]
        public async Task GetById_WhenProvidingExistingId_ShouldReturnOk()
        {
            // Arrange
            const int existingId = 2;

            // Act
            var result = await _instrumentController.GetInstrumentById(existingId);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeAssignableTo<InstrumentReadMinimalDto>().Subject;

            payload.Id.Should().Be(existingId);
            payload.Name.Should().Be("Sonor 14\"x6.5\" Chrome over Brass Sn.");
            payload.Brand.Should().Be("Sonor");
            payload.CategoryId.Should().Be(3);
            payload.Description.Should().Be("A cool snare drum with a less cool description");
        }

        [Test]
        public async Task GetById_WhenProvidingNonexistentId_ShouldReturnNotFound()
        {
            // Arrange
            const int nonexistentId = 999;

            // Act
            var result = await _instrumentController.GetInstrumentById(nonexistentId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task Create_WhenProvidingValidData_ShouldReturnOk()
        {
            // Arrange
            var dto = new InstrumentCreateDto
            {
                Name = "Yamaha Recording Custom 5-piece Drum Set",
                Brand = "Yamaha",
                CategoryId = 4, // Acoustic Drum Sets exists
                Description = "A high-end drum set for professional recording",
                CustomAttributes = []
            };

            // Act
            var result = await _instrumentController.CreateInstrument(dto);

            // Assert
            var createdAt = result.Should().BeOfType<CreatedAtActionResult>().Subject;

            createdAt.RouteValues.Should().NotBeNull();
            createdAt.RouteValues.Keys.Should().Contain("id");
            createdAt.RouteValues["id"].Should().Be(4);

            var payload = createdAt.Value.Should().BeAssignableTo<InstrumentReadMinimalDto>().Subject;

            payload.Id.Should().Be(4);
            payload.Name.Should().Be(dto.Name);
            payload.Brand.Should().Be(dto.Brand);
            payload.CategoryId.Should().Be(dto.CategoryId);
            payload.Description.Should().Be(dto.Description);
            payload.Attributes.Count.Should().Be(0);
        }

        [Test]
        public async Task Create_WhenNoCategoryWithProvidedCategoryIdExists_ShouldReturnBadRequest()
        {
            // Arrange
            var dto = new InstrumentCreateDto
            {
                Name = "Yamaha Recording Custom 5-piece Drum Set",
                Brand = "Yamaha",
                CategoryId = 999, // Nonexistent category
                Description = "A high-end drum set for professional recording",
                CustomAttributes = []
            };

            // Act
            var result = await _instrumentController.CreateInstrument(dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<InstrumentCreateBadRequestResponseDto>().Subject;

            payload.Message.Should().Be("Associated category does not exist.");
        }

        [Test]
        public async Task Update_WhenProvidingValidData_ShouldReturnNoContent()
        {
            // Arrange
            const int existingId = 1;
            var dto = new InstrumentUpdateDto
            {
                Id = existingId,
                Name = "Updated Instrument Name",
                Brand = "Updated Brand",
                CategoryId = 4, // Acoustic Drum Sets exists
                Description = "Updated Description",
                CustomAttributes = []
            };

            // Act
            var result = await _instrumentController.UpdateInstrument(existingId, dto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task Update_WhenIdsInPathAndBodyDoNotMatch_ShouldReturnBadRequest()
        {
            // Arrange
            const int existingId = 1;
            var dto = new InstrumentUpdateDto
            {
                Id = existingId + 1,
                Name = "Updated Instrument Name",
                Brand = "Updated Brand",
                CategoryId = 4, // Acoustic Drum Sets exists
                Description = "Updated Description",
                CustomAttributes = []
            };

            // Act
            var result = await _instrumentController.UpdateInstrument(existingId, dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<InstrumentUpdateBadRequestResponseDto>().Subject;

            payload.Message.Should().Be("Instrument Id mismatch between path and body.");
        }

        [Test]
        public async Task Update_WhenNoCategoryWithProvidedCategoryIdExists_ShouldReturnBadRequest()
        {
            // Arrange
            const int existingId = 1;
            var dto = new InstrumentUpdateDto
            {
                Id = existingId,
                Name = "Updated Instrument Name",
                Brand = "Updated Brand",
                CategoryId = 999, // Nonexistent category
                Description = "Updated Description",
                CustomAttributes = []
            };

            // Act
            var result = await _instrumentController.UpdateInstrument(existingId, dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<InstrumentUpdateBadRequestResponseDto>().Subject;

            payload.Message.Should().Be("Associated category does not exist.");
        }

        [Test]
        public async Task Update_WhenNoInstrumentWithProvidedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int nonexistentId = 999;
            var dto = new InstrumentUpdateDto
            {
                Id = nonexistentId,
                Name = "Updated Instrument Name",
                Brand = "Updated Brand",
                CategoryId = 4, // Acoustic Drum Sets exists
                Description = "Updated Description",
                CustomAttributes = []
            };

            // Act
            var result = await _instrumentController.UpdateInstrument(nonexistentId, dto);

            // Assert
            var badRequest = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<InstrumentUpdateNotFoundResponseDto>().Subject;

            payload.Message.Should().Be("No instrument with provided Id exists.");
        }

        [Test]
        public async Task GetAttributesOfInstrument_WhenProvidingIdOfExistingInstrumentWithoutAttributeValues_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            const int existingId = 1;

            // Act
            var result = await _instrumentController.GetAttributesOfInstrument(existingId);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeAssignableTo<IEnumerable<InstrumentAttributeValueReadDetailedDto>>().Subject;

            var list = payload as List<InstrumentAttributeValueReadDetailedDto>;

            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }

        [Test]
        public async Task GetAttributesOfInstrument_WhenProvidingIdOfExistingInstrumentWithAttributeValues_ShouldReturnOkWithList()
        {
            // Arrange
            const int existingId = 1;
            await SeedAttributeDefinitionsAndValuesForInstrument(existingId);

            // Act
            var result = await _instrumentController.GetAttributesOfInstrument(existingId);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeAssignableTo<IEnumerable<InstrumentAttributeValueReadDetailedDto>>().Subject;

            var list = payload as List<InstrumentAttributeValueReadDetailedDto>;

            list.Should().NotBeNull();
            list.Count.Should().Be(2);

            list.Select(av => av.Id).Should().Contain([1, 2]);
            list.Select(av => av.InstrumentId).Should().OnlyContain(iId => iId == existingId);
            list.Select(av => av.AttributeDefinitionId).Should().Contain([1, 2]);
            // Assert against the name of the attribute definition, which ensures that the property is returned
            list.Select(av => av.AttributeDefinition.Name).Should().Contain(
            [
                "Diameter",
                "Material"
            ]);
            list.Select(av => av.Value).Should().Contain(["14.0", "Black Nickel Brass"]);
        }

        [Test]
        public async Task GetAttributesOfInstrument_WhenNoInstrumentWithProviedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int nonexistentId = 999;

            // Act
            var result = await _instrumentController.GetAttributesOfInstrument(nonexistentId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task AddAttributeToInstrument_WhenProvidingValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            const int instrumentId = 1;
            await SeedAttributeDefinitionsAndValuesForInstrument(instrumentId);

            var dto = new InstrumentAttributeValueCreateDto
            {
                InstrumentId = instrumentId,
                AttributeDefinitionId = 2, // Material
                Value = "Another Material Attribute Value"
            };

            // Act
            var result = await _instrumentController.AddAttributeToInstrument(instrumentId, dto);

            // Assert
            var createdAt = result.Should().BeOfType<CreatedAtActionResult>().Subject;

            createdAt.RouteValues.Should().NotBeNull();
            createdAt.RouteValues.Keys.Should().Contain("id");
            createdAt.RouteValues["id"].Should().Be(3);

            var payload = createdAt.Value.Should().BeOfType<InstrumentAttributeValueReadMinimalDto>().Subject;
            payload.Id.Should().Be(3);
            payload.AttributeDefinitionId.Should().Be(dto.AttributeDefinitionId);
            payload.InstrumentId.Should().Be(instrumentId);
            payload.Value.Should().Be(dto.Value);
        }

        private async Task SeedAttributeDefinitionsAndValuesForInstrument(int instrumentId)
        {

            // Add attribute definitions to the database
            _dbContext.AttributeDefinitions.AddRange(
                new AttributeDefinition
                {
                    Id = 1,
                    Name = "Diameter",
                    DataType = AttributeDefinitionDataType.Decimal,
                    CategoryId = 2,
                    Category = (await _dbContext.Categories.FindAsync(3))!
                },
                new AttributeDefinition
                {
                    Id = 2,
                    Name = "Material",
                    DataType = AttributeDefinitionDataType.String,
                    CategoryId = 2,
                    Category = (await _dbContext.Categories.FindAsync(3))!
                }
            );

            // Add attribute values to db context
            var iav1 = new InstrumentAttributeValue
            {
                Id = 1,
                InstrumentId = instrumentId,
                Instrument = (await _dbContext.Instruments.FindAsync(instrumentId))!,
                AttributeDefinitionId = 1,
                AttributeDefinition = (await _dbContext.AttributeDefinitions.FindAsync(1))!,
                Value = "14.0"
            };

            var iav2 = new InstrumentAttributeValue
            {
                Id = 2,
                InstrumentId = instrumentId,
                Instrument = (await _dbContext.Instruments.FindAsync(instrumentId))!,
                AttributeDefinitionId = 2,
                AttributeDefinition = (await _dbContext.AttributeDefinitions.FindAsync(2))!,
                Value = "Black Nickel Brass"
            };

            _dbContext.InstrumentAttributeValues.AddRange(iav1, iav2);

            await _dbContext.SaveChangesAsync();
        }
    }
}
