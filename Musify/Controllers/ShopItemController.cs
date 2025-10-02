using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
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
        public async Task<IActionResult> CreateShopItem([FromBody] ShopItem shopItem)
        {
            if (shopItem == null)
            {
                return BadRequest("Shop item cannot be null.");
            }

            // Check if the associated Instrument exists
            var instrument = await _dbContext.Instruments.FindAsync(shopItem.InstrumentId);
            if (instrument == null)
            {
                return BadRequest("Associated instrument does not exist.");
            }

            _dbContext.ShopItems.Add(shopItem);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetShopItemById), new { id = shopItem.Id }, shopItem);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> UpdateShopItem(int id, [FromBody] ShopItem shopItem)
        {
            if (shopItem == null || shopItem.Id != id)
            {
                return BadRequest("Shop item data is invalid.");
            }

            var existingShopItem = await _dbContext.ShopItems.FindAsync(id);
            if (existingShopItem == null)
            {
                return NotFound();
            }

            existingShopItem.InstrumentId = shopItem.InstrumentId;
            existingShopItem.Price = shopItem.Price;
            existingShopItem.Stock = shopItem.Stock;
            existingShopItem.Condition = shopItem.Condition;

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
