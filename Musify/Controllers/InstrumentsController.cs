using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Dtos.AttributeDefinitionDtos;
using Musify.Dtos.AttributeValueDtos;
using Musify.Dtos.InstrumentDtos;
using Musify.Models;

namespace Musify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstrumentsController : ControllerBase
    {
        private MusifyDbContext _dbContext;

        public InstrumentsController(MusifyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Instrument>>> GetAllInstruments()
        {
            var instruments = await _dbContext.Instruments.ToListAsync();
            return Ok(instruments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Instrument>> GetInstrumentById(int id)
        {
            var instrument = await _dbContext.Instruments.FindAsync(id);
            if (instrument == null)
            {
                return NotFound();
            }
            return Ok(instrument);
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<ActionResult<Instrument>> CreateInstrument([FromBody] InstrumentCreateDto instrumentDto)
        {
            var category = await _dbContext.Categories.FindAsync(instrumentDto.CategoryId);
            if (category == null)
            {
                return BadRequest(new { Message = "Associated category does not exist." });
            }

            var newInstrument = new Instrument
            {
                Name = instrumentDto.Name,
                Brand = instrumentDto.Brand,
                CategoryId = instrumentDto.CategoryId,
                Category = category,
                Description = instrumentDto.Description
            };

            _dbContext.Instruments.Add(newInstrument);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInstrumentById), new { id = newInstrument.Id }, instrumentDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<ActionResult<Instrument>> UpdateInstrument(int id, [FromBody] Instrument instrument)
        {
            if (instrument.Id != id)
            {
                return BadRequest(new { Message = "Instrument id is invalid." });
            }

            var existingInstrument = await _dbContext.Instruments.FindAsync(id);
            if (existingInstrument == null)
            {
                return NotFound();
            }

            existingInstrument.Name = instrument.Name;
            existingInstrument.Brand = instrument.Brand;
            existingInstrument.CategoryId = instrument.CategoryId;
            existingInstrument.Description = instrument.Description;

            _dbContext.Instruments.Update(existingInstrument);
            await _dbContext.SaveChangesAsync();
            return Ok(existingInstrument);
        }

        [HttpGet("{id}/attributes")]
        public async Task<ActionResult<IEnumerable<InstrumentAttributeValueReadDetailedDto>>> GetAttributesForInstrument(int id)
        {
            var instrument = await _dbContext.Instruments
                .Include(i => i.Attributes)
                .ThenInclude(av => av.AttributeDefinition)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instrument == null)
            {
                return NotFound();
            }

            List<InstrumentAttributeValueReadDetailedDto> attributeDtos = [];
            foreach (var attr in instrument.Attributes)
            {
                attributeDtos.Add(new InstrumentAttributeValueReadDetailedDto
                {
                    Id = attr.Id,
                    InstrumentId = attr.InstrumentId,
                    AttributeDefinitionId = attr.AttributeDefinitionId,
                    AttributeDefinition = new AttributeDefinitionReadMinimalDto
                    {
                        Id = attr.AttributeDefinition.Id,
                        Name = attr.AttributeDefinition.Name,
                        DataType = attr.AttributeDefinition.DataType,
                        CategoryId = attr.AttributeDefinition.CategoryId
                    },
                    Value = attr.Value
                });
            }

            return Ok(attributeDtos);
        }

        [HttpPost("{id}/attributes")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<ActionResult<Instrument>> AddAttributeToInstrument(int id, [FromBody] InstrumentAttributeValueCreateDto attribute)
        {
            if (id != attribute.InstrumentId)
            {
                return BadRequest(new { Message = "Instrument id is invalid." });
            }

            var instrument = await _dbContext.Instruments.Include(i => i.Attributes).FirstOrDefaultAsync(i => i.Id == id);
            if (instrument == null)
            {
                return NotFound(new { Message = "The specified instrument does not exist." });
            }

            var attributeDefinition = await _dbContext.AttributeDefinitions.FindAsync(attribute.AttributeDefinitionId);
            if (attributeDefinition == null)
            {
                return BadRequest(new { Message = "Associated attribute definition does not exist." });
            }

            var attributeValue = new InstrumentAttributeValue
            {
                InstrumentId = id,
                Instrument = instrument,
                AttributeDefinitionId = attribute.AttributeDefinitionId,
                AttributeDefinition = attributeDefinition,
                Value = attribute.Value
            };


            instrument.Attributes.Add(attributeValue);

            _dbContext.Instruments.Update(instrument);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(
                nameof(GetInstrumentById),
                new { id = instrument.Id },
                new InstrumentAttributeValueReadMinimalDto
                {
                    Id = attributeValue.Id,
                    InstrumentId = attributeValue.InstrumentId,
                    AttributeDefinitionId = attributeValue.AttributeDefinitionId,
                    Value = attributeValue.Value
                });
        }

        [HttpPut("{instrumentId}/attributes/{attributeId}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<ActionResult<InstrumentAttributeValue>> UpdateAttributeOfInstrument(
            int instrumentId,
            int attributeId,
            [FromBody] InstrumentAttributeValueUpdateDto attribute
        )
        {
            if (attribute.Id != attributeId)
            {
                return BadRequest(new { Message = "Attribute id is invalid." });
            }

            if (attribute.InstrumentId != instrumentId)
            {
                return BadRequest(new { Message = "Instrument id is invalid." });
            }

            var instrument = await _dbContext.Instruments.
                Include(i => i.Attributes)
                .ThenInclude(av => av.AttributeDefinition)
                .FirstOrDefaultAsync(i => i.Id == instrumentId);

            if (instrument == null)
            {
                return NotFound(new { Message = "The specified instrument does not exist." });
            }

            var existingAttribute = instrument.Attributes.FirstOrDefault(a => a.Id == attributeId);
            if (existingAttribute == null)
            {
                return NotFound(new { Message = "The specified attribute does not exist." });
            }

            // Check if the new AttributeDefinitionId exists
            var attributeDefinition = await _dbContext.AttributeDefinitions.FindAsync(attribute.AttributeDefinitionId);
            if (attributeDefinition == null)
            {
                return BadRequest(new { Message = "The newly associated attribute definition does not exist." });
            }

            existingAttribute.AttributeDefinitionId = attribute.AttributeDefinitionId;
            existingAttribute.Value = attribute.Value;

            _dbContext.InstrumentAttributeValues.Update(existingAttribute);
            await _dbContext.SaveChangesAsync();

            // Construct after SaveChanges to ensure that correct navigation properties are loaded
            existingAttribute = await _dbContext.InstrumentAttributeValues
                .Include(av => av.AttributeDefinition)
                .FirstOrDefaultAsync(av => av.Id == attributeId);

            var attributeValueReadDto = new InstrumentAttributeValueReadDetailedDto
            {
                Id = existingAttribute!.Id,
                InstrumentId = existingAttribute.InstrumentId,
                AttributeDefinitionId = existingAttribute.AttributeDefinitionId,
                AttributeDefinition = new AttributeDefinitionReadMinimalDto
                {
                    Id = existingAttribute.AttributeDefinition.Id,
                    Name = existingAttribute.AttributeDefinition.Name,
                    DataType = existingAttribute.AttributeDefinition.DataType,
                    CategoryId = existingAttribute.AttributeDefinition.CategoryId
                },
                Value = existingAttribute.Value
            };

            return Ok(attributeValueReadDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteInstrument(int id)
        {
            var instrument = await _dbContext.Instruments.FindAsync(id);
            if (instrument == null)
            {
                return NotFound();
            }

            _dbContext.Instruments.Remove(instrument);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
