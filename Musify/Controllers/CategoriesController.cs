using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Dtos;
using Musify.Dtos.CategoryDtos;
using Musify.Models;
using Musify.Services;

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
    public class CategoriesController : ControllerBase
    {
        private readonly MusifyDbContext _dbContext;
        
        private readonly ICategoryTreeService _categoryTreeService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CategoriesController"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used to interact with the Musify database.</param>
        /// <param name="categoryTreeService">The category tree service used to build and return category trees.</param>
        public CategoriesController(MusifyDbContext dbContext, ICategoryTreeService categoryTreeService)
        {
            _dbContext = dbContext;
            _categoryTreeService = categoryTreeService;
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
            List<CategoryReadDto> categoryDtos =
                categories.Select(c =>
                    new CategoryReadDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ParentId = c.ParentId
                    }).ToList();

            return Ok(categoryDtos);
        }

        /// <summary>
        ///     Retrieves top level <see cref="Category"/> entities.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The list of top level categories.</returns>
        [HttpGet("TopLevel")]
        public async Task<IActionResult> GetTopLevelCategories(CancellationToken cancellationToken)
        {
            var categories = await _categoryTreeService.GetTopLevelCategoriesAsync(cancellationToken);
            return Ok(categories);
        }

        /// <summary>
        ///     Retrieves a <see cref="Category"/> by its unique identifier.
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
                return NotFound(new SimpleMessageDto { Message = $"No category with Id {id} was found." });
            }
            return Ok(new CategoryReadDto
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId
            });
        }

        /// <summary>
        ///     Retrieves the <see cref="Category"/> tree with the category identified by
        ///     <paramref name="id"/> as the root.
        /// </summary>
        /// <param name="id">The unique identifier of the root category.</param>
        /// <param name="cancellationToken">The cancellation token for this operation.</param>
        /// <returns>The flattened Category tree as a list.</returns>
        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildrenCategoriesById(int id, CancellationToken cancellationToken)
        {
            var categories = await _categoryTreeService.GetDescendantCategoriesAsync(id, cancellationToken);
            // Since the method above always returns the current category if it exists as the first element
            if (!categories.Any())
            {
                return NotFound(new SimpleMessageDto { Message = $"No category with Id {id} was found." });
            }
            return Ok(categories);
        }

        /// <summary>
        ///     Retrieves the <see cref="Category"/> tree with the Category identified by
        ///     <paramref name="id"/> as a leaf category.
        /// </summary>
        /// <param name="id">The unique identifier of the leaf category.</param>
        /// <param name="cancellationToken">The cancellation token for this operation.</param>
        /// <returns>The flattened Category tree as a list.</returns>
        [HttpGet("{id}/parents")]
        public async Task<IActionResult> GetParentCategoriesById(int id, CancellationToken cancellationToken)
        {
            var categories = await _categoryTreeService.GetAncestorCategoriesAsync(id, cancellationToken);
            // Since the method above always returns the current category if it exists as the last element
            if (!categories.Any())
            {
                return NotFound(new SimpleMessageDto { Message = $"No category with Id {id} was found." });
            }
            return Ok(categories);
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
                    return BadRequest(new SimpleMessageDto
                        { Message = "Parent category does not exist." });
                }
            }

            var newCategory = new Category
            {
                Name = categoryDto.Name,
                ParentId = categoryDto.ParentId
            };

            _dbContext.Categories.Add(newCategory);
            await _dbContext.SaveChangesAsync();

            var returnedCategoryDto = new CategoryReadDto
            {
                Id = newCategory.Id,
                Name = newCategory.Name,
                ParentId = newCategory.ParentId
            };
            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.Id }, returnedCategoryDto);
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
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDto category)
        {
            if (id != category.Id)
            {
                return BadRequest(new SimpleMessageDto
                    { Message = "Category Id mismatch between path and body." });
            }

            var existingCategory = await _dbContext.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new SimpleMessageDto { Message = "Category Id is invalid."});
            }

            // Check if parent category exists if ParentId is set
            if (category.ParentId.HasValue)
            {
                var parentCategory = await _dbContext.Categories.FindAsync(category.ParentId.Value);
                if (parentCategory == null)
                {
                    return BadRequest(new SimpleMessageDto
                        { Message = "Parent category does not exist." });
                }
            }

            existingCategory.Name = category.Name;
            existingCategory.ParentId = category.ParentId;

            _dbContext.Categories.Update(existingCategory);
            await _dbContext.SaveChangesAsync();
            return NoContent();
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
                return NotFound(new SimpleMessageDto
                    { Message = "No category with the specified Id was found." });
            }

            // Check if the category has any child categories
            List<Category> childCategories = await _dbContext.Categories.Where(c => c.ParentId == id).ToListAsync();
            if (childCategories.Any())
            {
                return BadRequest(new CategoryDeleteBadRequestResponseDto
                {
                    Message = "Cannot delete category with child categories.",
                    ChildCategoryIds = childCategories.Select(cc => cc.Id)
                });
            }

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
