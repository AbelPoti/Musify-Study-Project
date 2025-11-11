using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Controllers;
using Musify.Data.DatabaseContext;
using Musify.Dtos.ShopItemDtos;
using Musify.Models;

namespace Musify.Tests.ControllerUnitTests
{
    [TestFixture]
    internal class ShopItemControllerTests
    {
        private MusifyDbContext _dbContext;
        private ShopItemController _shopItemController;


        [SetUp]
        public void Setup()
        {
            // Use a unique name per test class to isolate data between tests
            var options = new DbContextOptionsBuilder<MusifyDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new MusifyDbContext(options);

            SeedDatabase();

            _shopItemController = new ShopItemController(_dbContext);
        }

        private void SeedDatabase()
        {
            _dbContext.Database.EnsureCreated();

            // Seed categories (parent entities of instruments)
            _dbContext.Categories.AddRange(
                new Category { Id = 1, Name = "Drums and Percussion", ParentId = null },
                new Category { Id = 2, Name = "Snare Drums", ParentId = 1 },
                new Category { Id = 3, Name = "Brass Snare Drums", ParentId = 2 },
                new Category { Id = 4, Name = "Acoustic Drum Sets", ParentId = 1 }
            );

            // Seed instruments (parent entities of shop items)
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

            // Seed ShopItems
            _dbContext.ShopItems.AddRange(
                new ShopItem // New Dialtune Snare
                {
                    Id = 1,
                    InstrumentId = 1,
                    Instrument = _dbContext.Instruments.Find(1),
                    Price = 1000.0M,
                    Stock = 5,
                    Condition = ShopItemCondition.New
                },
                new ShopItem // B-Stock Dialtune Snare
                {
                    Id = 2,
                    InstrumentId = 1,
                    Instrument = _dbContext.Instruments.Find(1),
                    Price = 850.0M,
                    Stock = 1,
                    Condition = ShopItemCondition.BStock
                },
                new ShopItem // New Yamaha Drum Set
                {
                    Id = 3,
                    InstrumentId = 3,
                    Instrument = _dbContext.Instruments.Find(3),
                    Price = 2200.0M,
                    Stock = 3,
                    Condition = ShopItemCondition.New
                },
                new ShopItem // Used Sonor Snare
                {
                    Id = 4,
                    InstrumentId = 2,
                    Instrument = _dbContext.Instruments.Find(2),
                    Price = 600.0M,
                    Stock = 1,
                    Condition = ShopItemCondition.Used
                }
            );

            _dbContext.SaveChanges();
        }

        [Test]
        public async Task GetAll_ShouldReturnOkWithList()
        {
            // Arrange done in Setup
            // Act
            var result = await _shopItemController.GetAllShopItems();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeAssignableTo<IEnumerable<ShopItem>>().Subject;

            var list = payload as List<ShopItem>;

            list.Should().NotBeNull();
            list.Count.Should().Be(4);

            list.Select(shI => shI.Id).Should().Contain([1, 2, 3, 4]);
            list.Select(shI => shI.InstrumentId).Should().Contain([1, 1, 3, 2]);
            // Assert against a property of the instrument to ensure the relation was loaded and returned
            list.Select(shI => shI.Instrument!.Brand).Should().Contain(["Dialtune", "Dialtune", "Yamaha", "Sonor"]);
            list.Select(shI => shI.Price).Should().Contain([1000.0M, 850.0M, 2200.0M, 600.0M]);
            list.Select(shI => shI.Stock).Should().Contain([5, 1, 3, 1]);
            list.Select(shI => shI.Condition).Should().Contain(
            [
                ShopItemCondition.New,
                ShopItemCondition.BStock,
                ShopItemCondition.New,
                ShopItemCondition.Used
            ]);
        }

        [Test]
        public async Task GetById_WhenProvidedIdIsValid_ShouldReturnOk()
        {
            // Arrange
            const int shopItemId = 1;

            // Act
            var result = await _shopItemController.GetShopItemById(shopItemId);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ShopItem>().Subject;

            payload.Id.Should().Be(1);
            payload.InstrumentId.Should().Be(1);
            payload.Instrument.Should().Be(await _dbContext.Instruments.FindAsync(1));
        }

        [Test]
        public async Task GetById_WhenNoShopItemWithProvidedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int nonexistentId = 999;

            // Act
            var result = await _shopItemController.GetShopItemById(nonexistentId);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<GetShopItemByIdNotFoundResponseDto>().Subject;

            payload.Message.Should().Be("No shop item with the specified Id exists.");
        }

        [Test]
        public async Task Create_WhenProvidedDataIsValid_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var dto = new ShopItemCreateDto // Used Yamaha Set
            {
                InstrumentId = 3,
                Price = 1900.0M,
                Stock = 1,
                Condition = ShopItemCondition.Used
            };

            // Act
            var result = await _shopItemController.CreateShopItem(dto);

            // Assert
            var createdAt = result.Should().BeOfType<CreatedAtActionResult>().Subject;

            createdAt.RouteValues.Should().NotBeNull();
            createdAt.RouteValues.Keys.Should().Contain("id");
            createdAt.RouteValues["id"].Should().Be(5);

            var payload = createdAt.Value.Should().BeOfType<ShopItem>().Subject;
            payload.Id.Should().Be(5);
            payload.InstrumentId.Should().Be(dto.InstrumentId);
            payload.Instrument.Should().Be(await _dbContext.Instruments.FindAsync(dto.InstrumentId));
            payload.Price.Should().Be(dto.Price);
            payload.Stock.Should().Be(dto.Stock);
            payload.Condition.Should().Be(dto.Condition);
        }

        [Test]
        public async Task Create_WhenNoInstrumentWithProvidedInstrumentIdExists_ShouldReturnBadRequest()
        {
            // Arrange
            var dto = new ShopItemCreateDto
            {
                InstrumentId = 999, // Does not exist
                Price = 1900.0M,
                Stock = 1,
                Condition = ShopItemCondition.Used
            };

            // Act
            var result = await _shopItemController.CreateShopItem(dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Should().BeOfType<CreateShopItemBadRequestResponseDto>().Subject;

            payload.Message.Should().Be("Associated instrument does not exist.");
        }



        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
