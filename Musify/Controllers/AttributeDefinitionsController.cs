using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Dtos;
using Musify.Dtos.AttributeDefinitionDtos;
using Musify.Models;

namespace Musify.Controllers
{
    /// <summary>
    ///     Provides endpoints for managing attribute definitions in the system.
    /// </summary>
    /// <remarks>
    ///     This controller allows clients to perform CRUD operations on attribute definitions, 
    ///     including retrieving all attribute definitions, retrieving by ID or category, creating new definitions, 
    ///     updating existing ones, and deleting definitions.  Operations outside reading actions require administrative privileges.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class AttributeDefinitionsController : ControllerBase
    {
        private readonly MusifyDbContext _musifyDbContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AttributeDefinitionsController"/> class.
        /// </summary>
        /// <param
        ///     name="musifyDbContext">The database context used to interact with the Musify database./>.
        /// </param>
        public AttributeDefinitionsController(MusifyDbContext musifyDbContext)
        {
            _musifyDbContext = musifyDbContext;
        }

        /// <summary>
        ///     Retrieves the list of all existing <see cref="AttributeDefinition"/> entities.
        /// </summary>
        /// <remarks>
        ///     This operation requires admin or manager privileges.
        /// </remarks>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> response containing the list of all attribute definitions.
        /// </returns>
        [HttpGet]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> GetAllAttributeDefinitions()
        {
            var attributeDefinitions = await _musifyDbContext.AttributeDefinitions.Include(aD => aD.Category).ToListAsync();

            var attributeDefinitionDtos = attributeDefinitions.Select(aD =>
                new AttributeDefinitionReadDetailedDto
                {
                    Id = aD.Id,
                    Name = aD.Name,
                    DataType = aD.DataType,
                    CategoryId = aD.CategoryId,
                    Category = aD.Category
                }).ToList();
            return Ok(attributeDefinitionDtos);
        }

        /// <summary>
        ///     Retrieves an attribute definition by its unique identifier.
        /// </summary>
        /// <remarks>
        ///     Retrieves the <see cref="AttributeDefinition"/> corresponding to the provided <paramref name="id"/>>.
        ///     If the attribute definition does not exist, a <see cref="NotFoundResult"/> is returned.
        /// </remarks>
        /// <param name="id">The unique identifier of the attribute definition to retrieve.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> containing the attribute definition if found; otherwise, a <see
        ///     cref="NotFoundResult"/> if no attribute definition exists with the specified identifier.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttributeDefinitionById(int id)
        {
            var attributeDefinition =
                await _musifyDbContext.AttributeDefinitions.Include(aD => aD.Category)
                    .FirstOrDefaultAsync(aD => aD.Id == id);
            if (attributeDefinition == null)
            {
                return NotFound();
            }
            return Ok(new AttributeDefinitionReadDetailedDto
            {
                Id = attributeDefinition.Id,
                Name = attributeDefinition.Name,
                DataType = attributeDefinition.DataType,
                CategoryId = attributeDefinition.CategoryId,
                Category = attributeDefinition.Category
            });
        }

        /// <summary>
        ///     Retrieves all attribute definitions associated with a specific category.
        /// </summary>
        /// <remarks>
        ///     Retrieves a list of <see cref="AttributeDefinition"/> entities that belong to the category corresponding to the provided <paramref name="categoryId"/>.
        ///     If the provided category does not exist, a <see cref="NotFoundResult"/> is returned.
        /// </remarks>
        /// <param name="categoryId">The unique identifier of the desired category.</param>
        /// <returns>
        ///     An <see cref="IActionResult"/> containing the list of attribute definitions for the specified category if found;
        ///     otherwise, a <see cref="NotFoundResult"/> if no category exists with the specified identifier.
        /// </returns>
        [HttpGet("category/{categoryId}")]
        [Authorize(Roles = $"{UserRole.StoreManager}, {UserRole.WarehouseManager}, {UserRole.Admin}")]
        public async Task<IActionResult> GetAttributeDefinitionsByCategoryId(int categoryId)
        {
            var category = await _musifyDbContext.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound(new SimpleMessageDto
                    { Message = "Category not found." });
            }

            var attributeDefinitions = await _musifyDbContext.AttributeDefinitions
                .Where(ad => ad.CategoryId == categoryId)
                .Include(a => a.Category)
                .ToListAsync();

            List<AttributeDefinitionReadDetailedDto> attributeDtos = [];
            foreach (var attr in attributeDefinitions)
            {
                attributeDtos.Add(new AttributeDefinitionReadDetailedDto
                {
                    Id = attr.Id,
                    Name = attr.Name,
                    DataType = attr.DataType,
                    CategoryId = attr.CategoryId,
                    Category = attr.Category
                });
            }

            return Ok(attributeDtos);
        }

        /// <summary>
        ///     Creates an attribute definition based on the provided data.
        /// </summary>
        /// <remarks>
        ///     The provided <see cref="AttributeDefinitionCreateDto"/> is used to create a new <see cref="AttributeDefinition"/> entity in the system.
        ///     The category identifier provided in <paramref name="attributeDto"/> must correspond to an existing category;
        ///     otherwise, a <see cref="BadRequestObjectResult"/> is returned.
        ///     This operation requires admin privileges.
        /// </remarks>
        /// <param name="attributeDto">The DTO used to create and persist the attribute definition.</param>
        /// <returns>
        ///     A <see cref="CreatedAtActionResult"/> containing the created attribute definition if successful;
        ///     otherwise a <see cref="BadRequestObjectResult"/> if the associated category does not exist.
        /// </returns>
        [HttpPost]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> CreateAttributeDefinition([FromBody] AttributeDefinitionCreateDto attributeDto)
        {
            // Check if the associated Category exists
            var category = await _musifyDbContext.Categories.FindAsync(attributeDto.CategoryId);
            if (category == null)
            {
                return BadRequest(new SimpleMessageDto { Message = "Associated category does not exist." });
            }

            var newAttributeDefinition = new AttributeDefinition
            {
                Name = attributeDto.Name,
                DataType = attributeDto.DataType,
                CategoryId = attributeDto.CategoryId,
                Category = category
            };

            _musifyDbContext.AttributeDefinitions.Add(newAttributeDefinition);
            await _musifyDbContext.SaveChangesAsync();

            var createdAttributeDefinitionDto = new AttributeDefinitionReadDetailedDto
            {
                Id = newAttributeDefinition.Id,
                Name = newAttributeDefinition.Name,
                DataType = newAttributeDefinition.DataType,
                CategoryId = newAttributeDefinition.CategoryId,
                Category = newAttributeDefinition.Category
            };
            return CreatedAtAction(nameof(GetAttributeDefinitionById), new { id = newAttributeDefinition.Id }, createdAttributeDefinitionDto);
        }

        /// <summary>
        ///     Updates an existing attribute definition with the provided data.
        /// </summary>
        /// <remarks>
        ///     The provided <see cref="AttributeDefinitionUpdateDto"/> is used to update an existing <see cref="AttributeDefinition"/> entity in the system.
        ///     The <paramref name="id"/> in the URL must match the identifier in the DTO, and a corresponding attribute definition must exist;
        ///     otherwise, a <see cref="BadRequestObjectResult"/> is returned.
        ///     The category identifier must correspond to an existing category; otherwise, a <see cref="BadRequestObjectResult"/> is returned.
        ///     This operation requires admin privileges.
        /// </remarks>
        /// <param name="id">The unique identifier of the attribute definition to be updated.</param>
        /// <param name="attributeDto">The DTO which holds the updated attributes.</param>
        /// <returns>
        ///     A <see cref="NoContentResult"/> if the update is successful;
        ///     otherwise, a <see cref="BadRequestObjectResult"/> if the IDs do not match or the associated category does not exist,
        /// </returns>
        [HttpPut("{id}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> UpdateAttributeDefinition(int id, [FromBody] AttributeDefinitionUpdateDto attributeDto)
        {
            if (id != attributeDto.Id)
            {
                return BadRequest(new SimpleMessageDto { Message = "ID in URL does not match ID in body." });
            }

            var existingAttributeDefinition = await _musifyDbContext.AttributeDefinitions.FindAsync(attributeDto.Id);
            if (existingAttributeDefinition == null)
            {
                return NotFound(new SimpleMessageDto { Message = "Attribute definition not found." });
            }

            // Check if the associated Category exists
            var category = await _musifyDbContext.Categories.FindAsync(attributeDto.CategoryId);
            if (category == null)
            {
                return BadRequest(new SimpleMessageDto { Message = "Associated category does not exist." });
            }

            existingAttributeDefinition.Name = attributeDto.Name;
            existingAttributeDefinition.DataType = attributeDto.DataType;
            existingAttributeDefinition.CategoryId = attributeDto.CategoryId;
            existingAttributeDefinition.Category = category;

            _musifyDbContext.AttributeDefinitions.Update(existingAttributeDefinition);
            await _musifyDbContext.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        ///     Deletes an existing attribute definition by its unique identifier.
        /// </summary>
        /// <remarks>
        ///     Deletes an existing <see cref="AttributeDefinition"/> entity corresponding to the provided <paramref name="id"/>.
        ///     The provided <paramref name="id"/> must correspond to an existing attribute definition; otherwise, a <see cref="NotFoundResult"/> is returned.
        /// </remarks>
        /// <param name="id">The unique identifier of the attribute definition to be deleted.</param>
        /// <returns>
        ///     A <see cref="NoContentResult"/> if the deletion is successful;
        ///     otherwise, a <see cref="NotFoundResult"/> if no attribute definition exists with the specified identifier.
        /// </returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteAttributeDefinition(int id)
        {
            var attributeDefinition = await _musifyDbContext.AttributeDefinitions.FindAsync(id);
            if (attributeDefinition == null)
            {
                return NotFound(new SimpleMessageDto { Message = "Attribute definition not found." });
            }

            _musifyDbContext.AttributeDefinitions.Remove(attributeDefinition);
            await _musifyDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
