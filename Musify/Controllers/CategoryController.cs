using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Dtos.CategoryDtos;
using Musify.Models;

namespace Musify.Controllers
{
    /// <summary>
    ///     Provides endpoints for managing categories.
    /// </summary>
    /// <remarks>
    ///     This controller allows users to perform CRUD operations on categories.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private MusifyDbContext _dbContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CategoryController"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used to interact with the Musify database.</param>
        public CategoryController(MusifyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        ///     Retrieves the list of all existing <see cref="Category"/> entities.
        /// </summary>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> response containing the list of categories.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _dbContext.Categories.ToListAsync();
            return Ok(categories);
        }

        /// <summary>
        ///     Retrieves a category by its unique identifier.
        /// </summary>
        /// <remarks>
        ///     Retrieves the <see cref="Category"/> corresponding to the provided <paramref name="id"/>.
        ///     If the category does not exist, a <see cref="NotFoundResult"/> is returned.
        /// </remarks>
        /// <param name="id">The unique identifier of the category to retrieve.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> containing the category if found;
        ///     otherwise a <see cref="NotFoundResult"/> if no category exists with the specified identifier.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        /// <summary>
        ///     Creates a category based on the provided data.
        /// </summary>
        /// <remarks>
        ///     The provided <see cref="CategoryCreateDto"/> is used to create a new <see cref="Category"/> entity in the system.
        ///     The parent category given in <paramref name="categoryDto"/> must already exist, else a <see cref="BadRequestObjectResult"/> response is returned.
        ///     This operation requires admin privileges.
        /// </remarks>
        /// <param name="categoryDto">The DTO used to create and persist the category.</param>
        /// <returns>
        ///     A <see cref="CreatedAtActionResult"/> containing the created category if successful;
        ///     otherwise a <see cref="BadRequestObjectResult"/> if the input data is invalid.
        /// </returns>
        [Authorize(Roles = UserRole.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto categoryDto)
        {
            // Check if parent category exists if ParentId is set
            if (categoryDto.ParentId.HasValue)
            {
                var parentCategory = await _dbContext.Categories.FindAsync(categoryDto.ParentId.Value);
                if (parentCategory == null)
                {
                    return BadRequest(new { Message = "Parent category does not exist." });
                }
            }

            var newCategory = new Category
            {
                Name = categoryDto.Name,
                ParentId = categoryDto.ParentId
            };

            _dbContext.Categories.Add(newCategory);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.Id }, categoryDto);
        }

        /// <summary>
        ///     Updates an existing category with the provided data.
        /// </summary>
        /// <remarks>
        ///     The provided <see cref="Category"/> is used to update an existing <see cref="Category"/> entity.
        ///     The <paramref name="id"/> provided in the URL must match the identifier in the <paramref name="category"/> object,
        ///     and the corresponding category must already exist, else a <see cref="BadRequestObjectResult"/> response is returned.
        ///     The provided parent category identifier must also exist, else a <see cref="BadRequestObjectResult"/> response is returned.
        ///     This operation requires admin privileges.
        /// </remarks>
        /// <param name="id">The unique identifier of the category to be updated.</param>
        /// <param name="category">The DTO which holds the updated attributes.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> response if the update is successful;
        ///     otherwise a <see cref="BadRequestObjectResult"/> response if the input data is invalid.
        /// </returns>
        [Authorize(Roles = UserRole.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (id != category.Id)
            {
                return BadRequest(new { Message = "Category id is invalid." });
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
                    return BadRequest(new { Message = "Parent category does not exist." });
                }
            }

            existingCategory.Name = category.Name;
            existingCategory.ParentId = category.ParentId;

            _dbContext.Categories.Update(existingCategory);
            await _dbContext.SaveChangesAsync();
            return Ok(existingCategory);
        }

        /// <summary>
        ///     Deletes an existing category by its unique identifier.
        /// </summary>
        /// <remarks>
        ///     Deletes an existing <see cref="Category"/> entity corresponding to the provided <paramref name="id"/>.
        ///     The provided identifier must correspond to an existing category; and the category must not have existing child categories;
        ///     otherwise, a <see cref="NotFoundResult"/> is returned.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns>
        ///     A <see cref="NoContentResult"/> if the deletion is successful;
        ///     otherwise a <see cref="NotFoundResult"/> response.
        /// </returns>
        [Authorize(Roles = UserRole.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
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
                return BadRequest(new { Message = "Cannot delete category with child categories." });
            }

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
