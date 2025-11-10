using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Controllers;
using Musify.Data.DatabaseContext;
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

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
