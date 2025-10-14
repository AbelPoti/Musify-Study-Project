using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Dtos.AttributeDefinitionDtos;
using Musify.Models;

namespace Musify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttributeDefinitionsController : ControllerBase
    {
        private MusifyDbContext _musifyDbContext;

        public AttributeDefinitionsController(MusifyDbContext musifyDbContext)
        {
            _musifyDbContext = musifyDbContext;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllAttributeDefinitions()
        {
            var attributeDefinitions = await _musifyDbContext.AttributeDefinitions.ToListAsync();
            return Ok(attributeDefinitions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttributeDefinitionById(int id)
        {
            var attributeDefinition = await _musifyDbContext.AttributeDefinitions.FindAsync(id);
            if (attributeDefinition == null)
            {
                return NotFound();
            }
            return Ok(attributeDefinition);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetAttributeDefinitionByCategoryId(int categoryId)
        {
            var attributeDefinitions = await _musifyDbContext.AttributeDefinitions
                .Where(ad => ad.CategoryId == categoryId)
                .ToListAsync();
            return Ok(attributeDefinitions);
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> CreateAttributeDefinition([FromBody] AttributeDefinitionCreateDto attributeDto)
        {
            // Check if the associated Category exists
            var category = await _musifyDbContext.Categories.FindAsync(attributeDto.CategoryId);
            if (category == null)
            {
                return BadRequest(new { Message = "Associated category does not exist." });
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
            return CreatedAtAction(nameof(GetAttributeDefinitionById), new { id = newAttributeDefinition.Id }, newAttributeDefinition);
        }

        [HttpPut]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> UpdateAttributeDefinition([FromBody] AttributeDefinitionUpdateDto attributeDto)
        {
            var existingAttributeDefinition = await _musifyDbContext.AttributeDefinitions.FindAsync(attributeDto.Id);
            if (existingAttributeDefinition == null)
            {
                return NotFound("Attribute definition not found.");
            }

            // Check if the associated Category exists
            var category = await _musifyDbContext.Categories.FindAsync(attributeDto.CategoryId);
            if (category == null)
            {
                return BadRequest(new { Message = "Associated category does not exist." });
            }

            existingAttributeDefinition.Name = attributeDto.Name;
            existingAttributeDefinition.DataType = attributeDto.DataType;
            existingAttributeDefinition.CategoryId = attributeDto.CategoryId;
            existingAttributeDefinition.Category = category;

            _musifyDbContext.AttributeDefinitions.Update(existingAttributeDefinition);
            await _musifyDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteAttributeDefinition(int id)
        {
            var attributeDefinition = await _musifyDbContext.AttributeDefinitions.FindAsync(id);
            if (attributeDefinition == null)
            {
                return NotFound("Attribute definition not found.");
            }

            _musifyDbContext.AttributeDefinitions.Remove(attributeDefinition);
            await _musifyDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
