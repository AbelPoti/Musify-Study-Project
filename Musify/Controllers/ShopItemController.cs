using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Dtos.ShopItemDtos;
using Musify.Models;

namespace Musify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopItemController : ControllerBase
    {
        private MusifyDbContext _dbContext;

        public ShopItemController(MusifyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllShopItems()
        {
            var shopItems = await _dbContext.ShopItems.ToListAsync();

            List<ShopItemReadMinimalDto> shopItemDtos =
                shopItems.Select(sI => new ShopItemReadMinimalDto(sI.Id, sI.InstrumentId, sI.Price, sI.Stock, sI.Condition)).ToList();

            return Ok(shopItemDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShopItemById(int id)
        {
            var shopItem = await _dbContext.ShopItems.FindAsync(id);
            if (shopItem == null)
            {
                return NotFound(new ShopItemGetByIdNotFoundResponseDto { Message = "No shop item with the specified Id exists." });
            }
            return Ok(new ShopItemReadMinimalDto(shopItem.Id, shopItem.InstrumentId, shopItem.Price, shopItem.Stock, shopItem.Condition));
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> CreateShopItem([FromBody] ShopItemCreateDto shopItemDto)
        {
            // Check if the associated Instrument exists
            var instrument = await _dbContext.Instruments.FindAsync(shopItemDto.InstrumentId);
            if (instrument == null)
            {
                return BadRequest(new ShopItemCreateBadRequestResponseDto { Message = "Associated instrument does not exist." });
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

            var createdShopItemDto = new ShopItemReadMinimalDto(
                createdShopItem.Id, createdShopItem.InstrumentId, createdShopItem.Price, createdShopItem.Stock, createdShopItem.Condition);
            return CreatedAtAction(nameof(GetShopItemById), new { id = createdShopItemDto.Id }, createdShopItemDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> UpdateShopItem(int id, [FromBody] ShopItemUpdateDto shopItemDto)
        {
            if (shopItemDto.Id != id)
            {
                return BadRequest(new ShopItemUpdateBadRequestResponseDto { Message = "Shop item id mismatch between URL and body." });
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound(new ShopItemUpdateNotFoundResponseDto { Message = "No shop item with specified id exists." });
            }

            var referencedInstrument = await _dbContext.Instruments.FindAsync(shopItemDto.InstrumentId);
            if (referencedInstrument == null)
            {
                return BadRequest(new ShopItemUpdateBadRequestResponseDto { Message = "No instrument with newly provided instrumentId exists." });
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
                return BadRequest(new ShopItemPatchPropertyBadRequestResponseDto { Message = "Stock value must be strictly positive." });
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound(new ShopItemPatchPropertyNotFoundResponseDto { Message = "No shop item with the specified Id exists." });
            }

            existingShopItem.Stock = newStock;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();

            var patchedShopItemDto = new ShopItemReadMinimalDto(
                existingShopItem.Id, existingShopItem.InstrumentId, existingShopItem.Price, existingShopItem.Stock, existingShopItem.Condition);
            return Ok(patchedShopItemDto);
        }

        [HttpPatch("{id}/stock/increment")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> IncrementShopItemStock(int id, [FromBody] int incrementBy)
        {
            if (incrementBy <= 0)
            {
                return BadRequest(new ShopItemPatchPropertyBadRequestResponseDto { Message = "Increment value must be strictly positive." });
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound(new ShopItemPatchPropertyNotFoundResponseDto { Message = "No shop item with specified Id exists." });
            }

            existingShopItem.Stock += incrementBy;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();

            var patchedShopItemDto = new ShopItemReadMinimalDto(
                existingShopItem.Id, existingShopItem.InstrumentId, existingShopItem.Price, existingShopItem.Stock, existingShopItem.Condition);
            return Ok(patchedShopItemDto);
        }

        [HttpPatch("{id}/stock/decrement")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> DecrementShopItemStock(int id, [FromBody] int decrementBy)
        {
            if (decrementBy <= 0)
            {
                return BadRequest(new ShopItemPatchPropertyBadRequestResponseDto { Message = "Decrement value must be strictly positive." });
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound(new ShopItemPatchPropertyNotFoundResponseDto { Message = "No shop item with specified Id exists." });
            }

            if (decrementBy > existingShopItem.Stock)
            {
                return BadRequest(new ShopItemPatchPropertyBadRequestResponseDto { Message = "Decrement value exceeds current stock." });
            }

            existingShopItem.Stock -= decrementBy;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();

            var patchedShopItemDto = new ShopItemReadMinimalDto(
                existingShopItem.Id, existingShopItem.InstrumentId, existingShopItem.Price, existingShopItem.Stock, existingShopItem.Condition);
            return Ok(patchedShopItemDto);
        }

        [HttpPatch("{id}/price")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> PatchShopItemPrice(int id, [FromBody] decimal newPrice)
        {
            if (newPrice <= 0)
            {
                return BadRequest(new ShopItemPatchPropertyBadRequestResponseDto { Message = "Price must be strictly positive." });
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound(new ShopItemPatchPropertyNotFoundResponseDto { Message = "No shop item with the specified Id exists." });
            }

            existingShopItem.Price = newPrice;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();

            var patchedShopItemDto = new ShopItemReadMinimalDto(
                existingShopItem.Id, existingShopItem.InstrumentId, existingShopItem.Price, existingShopItem.Stock, existingShopItem.Condition);
            return Ok(patchedShopItemDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteShopItem(int id)
        {
            var shopItem = await _dbContext.ShopItems.FindAsync(id);
            if (shopItem == null)
            {
                return NotFound(new ShopItemDeleteNotFoundResponseDto { Message = "No shop item with specified Id exists." });
            }

            _dbContext.ShopItems.Remove(shopItem);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
