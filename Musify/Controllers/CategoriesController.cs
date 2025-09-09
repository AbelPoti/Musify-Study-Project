using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Models;

namespace Musify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private MusifyDbContext _dbContext;

        public CategoriesController(MusifyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            var categories = await _dbContext.Categories.ToListAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
        {
            if (category == null)
            {
                return BadRequest("Category cannot be null.");
            }

            // Check if parent category exists if ParentId is set
            if (category.ParentId.HasValue)
            {
                var parentCategory = await _dbContext.Categories.FindAsync(category.ParentId.Value);
                if (parentCategory == null)
                {
                    return BadRequest("Parent category does not exist.");
                }
            }

            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> UpdateCategory(int id, [FromBody] Category category)
        {
            if (category == null || id != category.Id)
            {
                return BadRequest("Category data is invalid.");
            }

            var existingCategory = await _dbContext.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            // Check if parent category exists if ParentId is set
            if (category.ParentId.HasValue)
            {
                var parentCategory = await _dbContext.Categories.FindAsync(category.ParentId.Value);
                if (parentCategory == null)
                {
                    return BadRequest("Parent category does not exist.");
                }
            }

            existingCategory.Name = category.Name;
            existingCategory.ParentId = category.ParentId;

            _dbContext.Categories.Update(existingCategory);
            await _dbContext.SaveChangesAsync();
            return Ok(existingCategory);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Check if the category has any child categories
            var childCategories = await _dbContext.Categories.Where(c => c.ParentId == id).ToListAsync();
            if (childCategories.Any())
            {
                return BadRequest("Cannot delete category with child categories.");
            }

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
