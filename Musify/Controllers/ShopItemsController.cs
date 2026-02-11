using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Musify.Data.DatabaseContext;
using Musify.Data.Query.QueryObjects;
using Musify.Dtos;
using Musify.Dtos.RequestDtos;
using Musify.Dtos.RequestDtos.FilterDtos;
using Musify.Dtos.ShopItemDtos;
using Musify.Models;

namespace Musify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopItemsController : ControllerBase
    {
        private MusifyDbContext _dbContext;
        
        // Custom queries class for performing specific queries
        private readonly IQueries<ShopItem, ShopItemFilterDto> _shopItemQueries;

        public ShopItemsController(
            MusifyDbContext dbContext,
            IQueries<ShopItem, ShopItemFilterDto> shopItemQueries)
        {
            _dbContext = dbContext;
            _shopItemQueries = shopItemQueries;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllShopItems(
            [FromQuery(Name = "")] PageRequest page,
            [FromQuery(Name = "")] SortRequest? sort,
            [FromQuery(Name = "")] ShopItemFilterDto? filter,
            CancellationToken cancellationToken)
        {
            var pagedShopItems = await _shopItemQueries.GetItemsAsync(page, sort, filter, cancellationToken);

            // Map the results to DTOs
            var dtoResults = pagedShopItems.Map(shopItem => new ShopItemReadMinimalDto
            {
                Id = shopItem.Id,
                InstrumentId = shopItem.InstrumentId,
                Price = shopItem.Price,
                Stock = shopItem.Stock,
                Condition = shopItem.Condition
            });

            return Ok(dtoResults);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShopItemById(int id)
        {
            var shopItem = await _dbContext.ShopItems.FindAsync(id);
            if (shopItem == null)
            {
                return NotFound(new SimpleMessageDto { Message = "No shop item with the specified Id exists." });
            }
            return Ok(new ShopItemReadMinimalDto
            {
                Id = shopItem.Id,
                InstrumentId = shopItem.InstrumentId,
                Price = shopItem.Price,
                Stock = shopItem.Stock,
                Condition = shopItem.Condition
            });
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> CreateShopItem([FromBody] ShopItemCreateDto shopItemDto)
        {
            // Check if the associated Instrument exists
            var instrument = await _dbContext.Instruments.FindAsync(shopItemDto.InstrumentId);
            if (instrument == null)
            {
                return BadRequest(new SimpleMessageDto { Message = "Associated instrument does not exist." });
            }

            var createdShopItem = new ShopItem
            {
                InstrumentId = shopItemDto.InstrumentId,
                Instrument = instrument,
                Price = shopItemDto.Price,
                Stock = shopItemDto.Stock,
                Condition = shopItemDto.Condition
            };

            _dbContext.ShopItems.Add(createdShopItem);
            await _dbContext.SaveChangesAsync();

            var createdShopItemDto = new ShopItemReadMinimalDto
            {
                Id = createdShopItem.Id,
                InstrumentId = createdShopItem.InstrumentId,
                Price = createdShopItem.Price,
                Stock = createdShopItem.Stock,
                Condition = createdShopItem.Condition
            };
            return CreatedAtAction(nameof(GetShopItemById), new { id = createdShopItemDto.Id }, createdShopItemDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> UpdateShopItem(int id, [FromBody] ShopItemUpdateDto shopItemDto)
        {
            if (shopItemDto.Id != id)
            {
                return BadRequest(new SimpleMessageDto { Message = "Shop item id mismatch between URL and body." });
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound(new SimpleMessageDto { Message = "No shop item with specified id exists." });
            }

            var referencedInstrument = await _dbContext.Instruments.FindAsync(shopItemDto.InstrumentId);
            if (referencedInstrument == null)
            {
                return BadRequest(new SimpleMessageDto { Message = "No instrument with newly provided instrumentId exists." });
            }

            existingShopItem.InstrumentId = shopItemDto.InstrumentId;
            existingShopItem.Price = shopItemDto.Price;
            existingShopItem.Stock = shopItemDto.Stock;
            existingShopItem.Condition = shopItemDto.Condition;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/stock")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> PatchShopItemStock(int id, [FromBody] int newStock)
        {
            if (newStock <= 0)
            {
                return BadRequest(new SimpleMessageDto { Message = "Stock value must be strictly positive." });
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound(new SimpleMessageDto { Message = "No shop item with the specified Id exists." });
            }

            existingShopItem.Stock = newStock;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();

            var patchedShopItemDto = new ShopItemReadMinimalDto
            {
                Id = existingShopItem.Id,
                InstrumentId = existingShopItem.InstrumentId,
                Price = existingShopItem.Price,
                Stock = existingShopItem.Stock,
                Condition = existingShopItem.Condition
            };
            return Ok(patchedShopItemDto);
        }

        [HttpPatch("{id}/stock/increment")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> IncrementShopItemStock(int id, [FromBody] int incrementBy)
        {
            if (incrementBy <= 0)
            {
                return BadRequest(new SimpleMessageDto { Message = "Increment value must be strictly positive." });
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound(new SimpleMessageDto { Message = "No shop item with specified Id exists." });
            }

            existingShopItem.Stock += incrementBy;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();

            var patchedShopItemDto = new ShopItemReadMinimalDto
            {
                Id = existingShopItem.Id,
                InstrumentId = existingShopItem.InstrumentId,
                Price = existingShopItem.Price,
                Stock = existingShopItem.Stock,
                Condition = existingShopItem.Condition
            };
            return Ok(patchedShopItemDto);
        }

        [HttpPatch("{id}/stock/decrement")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> DecrementShopItemStock(int id, [FromBody] int decrementBy)
        {
            if (decrementBy <= 0)
            {
                return BadRequest(new SimpleMessageDto { Message = "Decrement value must be strictly positive." });
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound(new SimpleMessageDto { Message = "No shop item with specified Id exists." });
            }

            if (decrementBy > existingShopItem.Stock)
            {
                return BadRequest(new SimpleMessageDto { Message = "Decrement value exceeds current stock." });
            }

            existingShopItem.Stock -= decrementBy;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();

            var patchedShopItemDto = new ShopItemReadMinimalDto
            {
                Id = existingShopItem.Id,
                InstrumentId = existingShopItem.InstrumentId,
                Price = existingShopItem.Price,
                Stock = existingShopItem.Stock,
                Condition = existingShopItem.Condition
            };
            return Ok(patchedShopItemDto);
        }

        [HttpPatch("{id}/price")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> PatchShopItemPrice(int id, [FromBody] decimal newPrice)
        {
            if (newPrice <= 0)
            {
                return BadRequest(new SimpleMessageDto { Message = "Price must be strictly positive." });
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound(new SimpleMessageDto { Message = "No shop item with the specified Id exists." });
            }

            existingShopItem.Price = newPrice;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();

            var patchedShopItemDto = new ShopItemReadMinimalDto
            {
                Id = existingShopItem.Id,
                InstrumentId = existingShopItem.InstrumentId,
                Price = existingShopItem.Price,
                Stock = existingShopItem.Stock,
                Condition = existingShopItem.Condition
            };
            return Ok(patchedShopItemDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteShopItem(int id)
        {
            var shopItem = await _dbContext.ShopItems.FindAsync(id);
            if (shopItem == null)
            {
                return NotFound(new SimpleMessageDto { Message = "No shop item with specified Id exists." });
            }

            _dbContext.ShopItems.Remove(shopItem);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
