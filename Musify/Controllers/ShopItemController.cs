using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Dtos;
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
        public async Task<ActionResult<IEnumerable<ShopItem>>> GetAllShopItems()
        {
            var shopItems = await _dbContext.ShopItems.ToListAsync();
            return Ok(shopItems);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShopItem>> GetShopItemById(int id)
        {
            var shopItem = await _dbContext.ShopItems.FindAsync(id);
            if (shopItem == null)
            {
                return NotFound();
            }
            return Ok(shopItem);
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> CreateShopItem([FromBody] ShopItemCreateDto shopItemDto)
        {
            if (shopItemDto == null)
            {
                return BadRequest("Shop item cannot be null.");
            }

            // Check if the associated Instrument exists
            var instrument = await _dbContext.Instruments.FindAsync(shopItemDto.InstrumentId);
            if (instrument == null)
            {
                return BadRequest("Associated instrument does not exist.");
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
            return CreatedAtAction(nameof(GetShopItemById), new { id = createdShopItem.Id }, createdShopItem);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> UpdateShopItem(int id, [FromBody] ShopItemUpdateDto shopItemDto)
        {
            if (shopItemDto == null || shopItemDto.Id != id)
            {
                return BadRequest("Shop item data is invalid.");
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound();
            }

            existingShopItem.InstrumentId = shopItemDto.InstrumentId;
            existingShopItem.Price = shopItemDto.Price;
            existingShopItem.Stock = shopItemDto.Stock;
            existingShopItem.Condition = shopItemDto.Condition;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();
            return Ok(existingShopItem);
        }

        [HttpPatch("{id}/stock")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> UpdateShopItemStock(int id, [FromBody] int newStock)
        {
            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound();
            }

            existingShopItem.Stock = newStock;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();
            return Ok(existingShopItem);
        }

        [HttpPatch("{id}/stock/increment")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> IncrementShopItemStock(int id, [FromBody] int incrementBy)
        {
            if (incrementBy <= 0)
            {
                return BadRequest("Increment value must be positive.");
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound();
            }

            existingShopItem.Stock += incrementBy;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();
            return Ok(existingShopItem);
        }

        [HttpPatch("{id}/stock/decrement")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> DecrementShopItemStock(int id, [FromBody] int decrementBy)
        {
            if (decrementBy <= 0)
            {
                return BadRequest("Decrement value most be positive.");
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound();
            }

            if (decrementBy > existingShopItem.Stock)
            {
                return BadRequest("Decrement value exceeds current stock.");
            }

            existingShopItem.Stock -= decrementBy;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();
            return Ok(existingShopItem);
        }

        [HttpPatch("{id}/price")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> UpdateShopItemPrice(int id, [FromBody] decimal newPrice)
        {
            if (newPrice <= 0)
            {
                return BadRequest("Price must be strictly positive.");
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound();
            }

            existingShopItem.Price = newPrice;

            _dbContext.ShopItems.Update(existingShopItem);
            await _dbContext.SaveChangesAsync();
            return Ok(existingShopItem);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteShopItem(int id)
        {
            var shopItem = await _dbContext.ShopItems.FindAsync(id);
            if (shopItem == null)
            {
                return NotFound();
            }

            _dbContext.ShopItems.Remove(shopItem);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
