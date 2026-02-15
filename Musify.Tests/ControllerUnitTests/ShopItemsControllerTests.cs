using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Controllers;
using Musify.Data.DatabaseContext;
using Musify.Data.Query.QueryObjects;
using Musify.Data.Query.QueryUtils;
using Musify.Data.Query.QueryUtils.QueryFilters;
using Musify.Dtos;
using Musify.Dtos.RequestDtos;
using Musify.Dtos.RequestDtos.FilterDtos;
using Musify.Dtos.ShopItemDtos;
using Musify.Models;

namespace Musify.Tests.ControllerUnitTests
{
    [TestFixture]
    internal class ShopItemsControllerTests
    {
        private MusifyDbContext _dbContext;
        private ShopItemQueries _shopItemQueries;
        private ShopItemsController _shopItemsController;
        
        [SetUp]
        public void Setup()
        {
            // Use a unique name per test class to isolate data between tests
            var options = new DbContextOptionsBuilder<MusifyDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new MusifyDbContext(options);
            _shopItemQueries = new ShopItemQueries(_dbContext, new ShopItemFiltering());

            SeedDatabase();

            _shopItemsController = new ShopItemsController(_dbContext, _shopItemQueries);
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
                    Instrument = _dbContext.Instruments.Find(1)!,
                    Price = 1000.0M,
                    Stock = 5,
                    Condition = ShopItemCondition.New
                },
                new ShopItem // B-Stock Dialtune Snare
                {
                    Id = 2,
                    InstrumentId = 1,
                    Instrument = _dbContext.Instruments.Find(1)!,
                    Price = 850.0M,
                    Stock = 1,
                    Condition = ShopItemCondition.BStock
                },
                new ShopItem // New Yamaha Drum Set
                {
                    Id = 3,
                    InstrumentId = 3,
                    Instrument = _dbContext.Instruments.Find(3)!,
                    Price = 2200.0M,
                    Stock = 3,
                    Condition = ShopItemCondition.New
                },
                new ShopItem // Used Sonor Snare
                {
                    Id = 4,
                    InstrumentId = 2,
                    Instrument = _dbContext.Instruments.Find(2)!,
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
            // Arrange
            PageRequest pageRequest = new PageRequest { Page = 1, PageSize = 3};
            SortRequest sortRequest = new SortRequest { Descending = false, SortBy = "price" };
            ShopItemFilterDto shopItemFilterDto = new ShopItemFilterDto
            {
                InstrumentFiter = new InstrumentFiterDto
                {
                    CategoryId = 1 // Drums root category
                },
                PriceFilter = new PriceFilterDto
                {
                    MaxPrice = 2100.0M,
                    MinPrice = 650.0M
                }
            };
            CancellationToken cancellationToken = CancellationToken.None;
            
            // Act
            var result = await _shopItemsController.GetAllShopItems(
                pageRequest,
                sortRequest,
                shopItemFilterDto,
                cancellationToken);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeAssignableTo<PagedResult<ShopItemReadMinimalDto>>().Subject;

            payload.TotalCount.Should().Be(2);
            payload.Page.Should().Be(1);
            payload.PageSize.Should().Be(3);
            
            var list = payload.Items as List<ShopItemReadMinimalDto>;
            
            list.Should().NotBeNull();
            list.Count.Should().Be(2);

            list.Select(shI => shI.Id).Should().Contain([1, 2]);
            list.Select(shI => shI.InstrumentId).Should().Contain([1, 1]);
            list.Select(shI => shI.Price).Should().Contain([1000.0M, 850.0M]);
            list.Select(shI => shI.Stock).Should().Contain([5, 1]);
            list.Select(shI => shI.Condition).Should().Contain(
            [
                ShopItemCondition.New,
                ShopItemCondition.BStock,
            ]);
            
            // Check for sort asc
            list[0].Id.Should().Be(2);
        }

        [Test]
        public async Task GetById_WhenProvidedIdIsValid_ShouldReturnOk()
        {
            // Arrange
            const int shopItemId = 1;

            // Act
            var result = await _shopItemsController.GetShopItemById(shopItemId);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ShopItemReadMinimalDto>().Subject;

            payload.Id.Should().Be(1);
            payload.InstrumentId.Should().Be(1);
        }

        [Test]
        public async Task GetById_WhenNoShopItemWithProvidedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int nonexistentId = 999;

            // Act
            var result = await _shopItemsController.GetShopItemById(nonexistentId);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<SimpleMessageDto>().Subject;

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
            var result = await _shopItemsController.CreateShopItem(dto);

            // Assert
            var createdAt = result.Should().BeOfType<CreatedAtActionResult>().Subject;

            createdAt.RouteValues.Should().NotBeNull();
            createdAt.RouteValues.Keys.Should().Contain("id");
            createdAt.RouteValues["id"].Should().Be(5);

            var payload = createdAt.Value.Should().BeOfType<ShopItemReadMinimalDto>().Subject;
            payload.Id.Should().Be(5);
            payload.InstrumentId.Should().Be(dto.InstrumentId);
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
            var result = await _shopItemsController.CreateShopItem(dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("Associated instrument does not exist.");
        }

        [Test]
        public async Task UpdateShopItem_WhenProvidedDataIsValid_ShouldReturnNoContent()
        {
            // Arrange
            const int shopItemId = 1;
            var dto = new ShopItemUpdateDto
            {
                Id = shopItemId,
                InstrumentId = 1,
                Price = 10000.0M,
                Stock = 2,
                Condition = ShopItemCondition.Used
            };

            // Act
            var result = await _shopItemsController.UpdateShopItem(shopItemId, dto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task UpdateShopItem_WhenIdsProvidedInPathAndBodyDoNotMatch_ShouldReturnBadRequest()
        {
            // Arrange
            const int shopItemId = 1;
            var dto = new ShopItemUpdateDto
            {
                Id = shopItemId + 1, // Mismatch
                InstrumentId = 1,
                Price = 10000.0M,
                Stock = 2,
                Condition = ShopItemCondition.Used
            };

            // Act
            var result = await _shopItemsController.UpdateShopItem(shopItemId, dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("Shop item id mismatch between URL and body.");
        }

        [Test]
        public async Task UpdateShopItem_WhenNoShopItemWithProvidedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int shopItemId = 999; // Does not exist
            var dto = new ShopItemUpdateDto
            {
                Id = shopItemId,
                InstrumentId = 1,
                Price = 10000.0M,
                Stock = 2,
                Condition = ShopItemCondition.Used
            };

            // Act
            var result = await _shopItemsController.UpdateShopItem(shopItemId, dto);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("No shop item with specified id exists.");
        }

        [Test]
        public async Task UpdateShopItem_WhenNoInstrumentWithProvidedNewInstrumentIdExists_ShouldReturnBadRequest()
        {
            // Arrange
            const int shopItemId = 1;
            var dto = new ShopItemUpdateDto
            {
                Id = shopItemId,
                InstrumentId = 999, // Does not exist
                Price = 10000.0M,
                Stock = 2,
                Condition = ShopItemCondition.Used
            };

            // Act
            var result = await _shopItemsController.UpdateShopItem(shopItemId, dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("No instrument with newly provided instrumentId exists.");
        }

        [Test]
        public async Task PatchShopItemPrice_WhenProvidedDataIsValid_ShouldReturnOk()
        {
            // Arrange
            const int shopItemId = 1;
            const decimal newPrice = 2000.0M;

            // Act
            var result = await _shopItemsController.PatchShopItemPrice(shopItemId, newPrice);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ShopItemReadMinimalDto>().Subject;

            payload.Id.Should().Be(shopItemId);
            payload.Price.Should().Be(newPrice);
        }

        [Test]
        public async Task PatchShopItemPrice_WhenNoShopItemWithProvidedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int shopItemId = 999; // Does not exist
            const decimal newPrice = 2000.0M;

            // Act
            var result = await _shopItemsController.PatchShopItemPrice(shopItemId, newPrice);

            // Assert
            var ok = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("No shop item with the specified Id exists.");
        }

        [Test]
        [TestCase(0.0d)]
        [TestCase(-1.0d)]
        public async Task PatchShopItemPrice_WhenProvidedPriceIsLEThanZero_ShouldReturnBadRequest(decimal newPrice)
        {
            // Arrange
            const int shopItemId = 1;

            // Act
            var result = await _shopItemsController.PatchShopItemPrice(shopItemId, newPrice);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("Price must be strictly positive.");
        }

        [Test]
        public async Task PatchSopItemStock_WhenProvidedDataIsValid_ShouldReturnOk()
        {
            // Arrange
            const int shopItemId = 1;
            const int newStock = 5;

            // Act
            var result = await _shopItemsController.PatchShopItemStock(shopItemId, newStock);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ShopItemReadMinimalDto>().Subject;

            payload.Id.Should().Be(shopItemId);
            payload.Stock.Should().Be(newStock);
        }

        [Test]
        public async Task PatchShopItemStock_WhenNoShopItemWithProvidedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int shopItemId = 999; // Does not exist
            const int newStock = 5;

            // Act
            var result = await _shopItemsController.PatchShopItemStock(shopItemId, newStock);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("No shop item with the specified Id exists.");
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task PatchShopItemStock_WhenProvidedStockIsLEThanZero_ShouldReturnBadRequest(int newStock)
        {
            // Arrange
            const int shopItemId = 1;

            // Act
            var result = await _shopItemsController.PatchShopItemStock(shopItemId, newStock);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("Stock value must be strictly positive.");
        }

        [Test]
        public async Task IncrementShopItemStock_WhenProvidedDataIsValid_ShouldReturnOk()
        {
            // Arrange
            const int shopItemId = 1;
            const int increment = 5;

            var shopItemPatched = await _dbContext.ShopItems.FindAsync(shopItemId);
            int currentStock = shopItemPatched!.Stock;

            // Act
            var result = await _shopItemsController.IncrementShopItemStock(shopItemId, increment);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ShopItemReadMinimalDto>().Subject;

            payload.Id.Should().Be(shopItemId);
            payload.Stock.Should().Be(currentStock + increment);
        }

        [Test]
        public async Task IncrementShopItemStock_WhenNoShopItemWithProvidedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int shopItemId = 999; // Does not exist
            const int increment = 5;

            // Act
            var result = await _shopItemsController.IncrementShopItemStock(shopItemId, increment);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("No shop item with specified Id exists.");
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task IncrementShopItemStock_WhenProvidedIncrementIsLEThanZero_ShouldReturnBadRequest(int increment)
        {
            // Arrange
            const int shopItemId = 1;

            // Act
            var result = await _shopItemsController.IncrementShopItemStock(shopItemId, increment);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("Increment value must be strictly positive.");
        }

        [Test]
        public async Task DecrementShopItemStock_WhenProvidedDataIsValid_ShouldReturnOk()
        {
            // Arrange
            const int shopItemId = 1; // Its stock is 5
            const int decrement = 4;

            var patchedShopItem = await _dbContext.ShopItems.FindAsync(shopItemId);
            int currentStock = patchedShopItem!.Stock;

            // Act
            var result = await _shopItemsController.DecrementShopItemStock(shopItemId, decrement);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ShopItemReadMinimalDto>().Subject;

            payload.Id.Should().Be(shopItemId);
            payload.Stock.Should().Be(currentStock - decrement);
        }

        [Test]
        public async Task DecrementShopItemStock_WhenNoShopItemWithProvidedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int shopItemId = 999; // Does not exist
            const int decrement = 4;

            // Act
            var result = await _shopItemsController.DecrementShopItemStock(shopItemId, decrement);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("No shop item with specified Id exists.");
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task DecrementShopItemStock_WhenProvidedDecrementValueIsLEThanZero_ShouldReturnBadRequest(int decrement)
        {
            // Arrange
            const int shopItemId = 1;

            // Act
            var result = await _shopItemsController.DecrementShopItemStock(shopItemId, decrement);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("Decrement value must be strictly positive.");
        }

        [Test]
        public async Task DecrementShopItemStock_WhenProvidedDecrementValueExceedsCurrentStock_ShouldReturnBadRequest()
        {
            // Arrange
            const int shopItemId = 1; // Current stock is 5
            const int decrement = 6;

            // Act
            var result = await _shopItemsController.DecrementShopItemStock(shopItemId, decrement);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("Decrement value exceeds current stock.");
        }

        [Test]
        public async Task Delete_WhenProvidedIdIsValid_ShouldReturnNoContent()
        {
            // Arrange
            const int shopItemId = 1;

            // Act
            var result = await _shopItemsController.DeleteShopItem(shopItemId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task Delete_WhenNoShopItemWithProvidedIdExists_ShouldReturnNotFound()
        {
            // Arrange
            const int shopItemId = 999; // Does not exist

            // Act
            var result = await _shopItemsController.DeleteShopItem(shopItemId);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<SimpleMessageDto>().Subject;

            payload.Message.Should().Be("No shop item with specified Id exists.");
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
