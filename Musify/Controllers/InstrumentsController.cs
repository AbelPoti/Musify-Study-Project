using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Data.Query.QueryObjects;
using Musify.Dtos;
using Musify.Dtos.AttributeDefinitionDtos;
using Musify.Dtos.AttributeValueDtos;
using Musify.Dtos.InstrumentDtos;
using Musify.Dtos.RequestDtos;
using Musify.Dtos.RequestDtos.FilterDtos;
using Musify.Models;

namespace Musify.Controllers
{
    /// <summary>
    ///     Provides endpoints for managing instruments.
    /// </summary>
    /// <remarks>
    ///     This controller allows users to perform CRUD operations on instruments.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class InstrumentsController : ControllerBase
    {
        private MusifyDbContext _dbContext;
        
        // Custom queries class for specific queries and page returning ones
        private readonly IQueries<Instrument, InstrumentFiterDto> _instrumentQueries;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InstrumentsController"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used to interact with the Musify database.</param>
        /// <param name="instrumentQueries">
        ///     The query object for specific queries not provided by <paramref name="dbContext"/>
        /// </param>
        public InstrumentsController(
            MusifyDbContext dbContext,
            IQueries<Instrument, InstrumentFiterDto> instrumentQueries)
        {
            _dbContext = dbContext;
            _instrumentQueries = instrumentQueries;
        }

        /// <summary>
        ///     Retrieves the list of all existing <see cref="Instrument"/> entities.
        /// </summary>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> response containing the list of categories.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAllInstruments(
            [FromQuery] PageRequest page,
            [FromQuery] SortRequest? sort,
            [FromQuery] InstrumentFiterDto? filter,
            CancellationToken cancellationToken)
        {
            var pagedInstruments = await _instrumentQueries.GetItemsAsync(page, sort, filter, cancellationToken);
            
            // Map the results to DTOs
            var dtoResult = pagedInstruments.Map(instrument => new InstrumentReadMinimalDto
            {
                Id = instrument.Id,
                Name = instrument.Name,
                Brand = instrument.Brand,
                CategoryId = instrument.CategoryId,
                Description = instrument.Description,
                Attributes = []
            });

            return Ok(dtoResult);
        }

        /// <summary>
        ///     Retrieves an instrument by its unique identifier.
        /// </summary>
        /// <remarks>
        ///     Retrieves the <see cref="Instrument"/> corresponding to the provided <paramref name="id"/>.
        ///     If the instrument does not exist, a <see cref="NotFoundResult"/> response is returned.
        /// </remarks>
        /// <param name="id">The unique identifier of the instrument to retrieve.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> containing the instrument if found;
        ///     otherwise a <see cref="NotFoundResult"/> if no instrument exists with the specified identifier.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInstrumentById(int id)
        {
            var instrument = await _dbContext.Instruments.FindAsync(id);
            if (instrument == null)
            {
                return NotFound();
            }

            var instrumentDto = new InstrumentReadMinimalDto
            {
                Id = instrument.Id,
                Name = instrument.Name,
                Brand = instrument.Brand,
                CategoryId = instrument.CategoryId,
                Description = instrument.Description,
                Attributes = []
            };

            return Ok(instrumentDto);
        }

        /// <summary>
        ///     Creates an instrument based on the provided data.
        /// </summary>
        /// <remarks>
        ///     The provided <see cref="InstrumentCreateDto"/> is used to create a new <see cref="Instrument"/> entity in the system.
        ///     The associated <see cref="Category"/> must exist; otherwise a <see cref="BadRequestObjectResult"/> response is returned.
        ///     This operation requires admin privileges.
        /// </remarks>
        /// <param name="instrumentDto">The DTO used to create and persist the category.</param>
        /// <returns>
        ///     A <see cref="CreatedAtActionResult"/> response containing the created instrument if successful;
        ///     otherwise a <see cref="BadRequestObjectResult"/> if the input data is invalid.
        /// </returns>
        [HttpPost]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> CreateInstrument([FromBody] InstrumentCreateDto instrumentDto)
        {
            var category = await _dbContext.Categories.FindAsync(instrumentDto.CategoryId);
            if (category == null)
            {
                return BadRequest(new SimpleMessageDto { Message = "Associated category does not exist." });
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

            var returnedInstrumentDto = new InstrumentReadMinimalDto
            {
                Id = newInstrument.Id,
                Name = newInstrument.Name,
                Brand = newInstrument.Brand,
                CategoryId = newInstrument.CategoryId,
                Description = newInstrument.Description,
                Attributes = []
            };
            return CreatedAtAction(nameof(GetInstrumentById), new { id = newInstrument.Id }, returnedInstrumentDto);
        }

        /// <summary>
        ///     Updates an existing instrument with the provided data.
        /// </summary>
        /// <remarks>
        ///     The provided <see cref="InstrumentUpdateDto"/> is used to update an existing <see cref="Instrument"/> entity in the system.
        ///     The <paramref name="id"/> provided in the URL must match the identifier in the <paramref name="instrumentDto"/> object,
        ///     The associated category must exist, else a <see cref="BadRequestObjectResult"/> response is returned.
        ///     This operation requires admin privileges.
        /// </remarks>
        /// <param name="id">The unique identifier of the instrument to be updated.</param>
        /// <param name="instrumentDto">The DTO which holds the updated attributes.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> response if the update is successful;
        ///     otherwise a <see cref="BadRequestObjectResult"/> if the input data is invalid.
        /// </returns>
        [HttpPut("{id}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> UpdateInstrument(int id, [FromBody] InstrumentUpdateDto instrumentDto)
        {
            if (instrumentDto.Id != id)
            {
                return BadRequest(new SimpleMessageDto { Message = "Instrument Id mismatch between path and body." });
            }

            var category = await _dbContext.Categories.FindAsync(instrumentDto.CategoryId);
            if (category == null)
            {
                return BadRequest(new SimpleMessageDto { Message = "Associated category does not exist." });
            }

            var existingInstrument = await _dbContext.Instruments.FindAsync(id);
            if (existingInstrument == null)
            {
                return NotFound(new SimpleMessageDto { Message = "No instrument with provided Id exists." });
            }

            existingInstrument.Name = instrumentDto.Name;
            existingInstrument.Brand = instrumentDto.Brand;
            existingInstrument.CategoryId = instrumentDto.CategoryId;
            existingInstrument.Description = instrumentDto.Description;

            _dbContext.Instruments.Update(existingInstrument);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        ///     Retrieves the list of attribute values associated with a specific instrument.
        /// </summary>
        /// <remarks>
        ///     Retrieves the list of <see cref="InstrumentAttributeValue"/> entities associated with the <see cref="Instrument"/> corrresponding
        ///     to the provided <paramref name="id"/>. If the instrument does not exist, a <see cref="NotFoundResult"/> response is returned.
        /// </remarks>
        /// <param name="id">The unique identifier of the instrument.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> containing the list of attribute values if the instrument is found;
        ///     otherwise a <see cref="NotFoundResult"/> if no instrument exists with the specified identifier.
        /// </returns>
        [HttpGet("{id}/attributes")]
        public async Task<IActionResult> GetAttributesOfInstrument(int id)
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
                        Id = attr.AttributeDefinitionId,
                        Name = attr.AttributeDefinition.Name,
                        DataType = attr.AttributeDefinition.DataType,
                        CategoryId = attr.AttributeDefinition.CategoryId
                    },
                    Value = attr.Value
                });
            }

            return Ok(attributeDtos);
        }


        /// <summary>
        ///     Adds a new attribute value to a specific instrument.
        /// </summary>
        /// <remarks>
        ///     Add a new <see cref="InstrumentAttributeValue"/> to the <see cref="Instrument"/> corresponding to the provided <paramref name="id"/>.
        ///     The provided id in the URL must match that in the <paramref name="attribute"/> object and the associated attribute definition must exist;
        ///     otherwise a <see cref="BadRequestObjectResult"/> response is returned.
        ///     The <see cref="AttributeDefinition"/> associated with the new attribute value must exist;
        ///     otherwise a <see cref="BadRequestObjectResult"/> response is returned.
        ///     This operation requires admin privileges.
        /// </remarks>
        /// <param name="id">The unique identifier of the instrument.</param>
        /// <param name="attribute">The DTO which holds the data used to create the new attribute value.</param>
        /// <returns>
        ///     A <see cref="CreatedAtActionResult"/> containing the created attribute value if successful;
        ///     A <see cref="BadRequestObjectResult"/> if the input data is invalid.
        /// </returns>
        [HttpPost("{id}/attributes")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> AddAttributeToInstrument(int id, [FromBody] InstrumentAttributeValueCreateDto attribute)
        {
            if (id != attribute.InstrumentId)
            {
                return BadRequest(new SimpleMessageDto { Message = "Instrument id mismatch between URL and body." });
            }

            var instrument = await _dbContext.Instruments
                .Include(i => i.Attributes)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (instrument == null)
            {
                return NotFound(new SimpleMessageDto { Message = "The specified instrument does not exist." });
            }

            var attributeDefinition = await _dbContext.AttributeDefinitions.FindAsync(attribute.AttributeDefinitionId);
            if (attributeDefinition == null)
            {
                return BadRequest(new SimpleMessageDto { Message = "Associated attribute definition does not exist." });
            }

            var attributeValue = new InstrumentAttributeValue
            {
                InstrumentId = id,
                Instrument = instrument,
                AttributeDefinitionId = attribute.AttributeDefinitionId,
                AttributeDefinition = attributeDefinition,
                Value = attribute.Value
            };

            _dbContext.InstrumentAttributeValues.Add(attributeValue);

            _dbContext.Instruments.Update(instrument);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(
                nameof(GetInstrumentById),
                new { id = attributeValue.Id },
                new InstrumentAttributeValueReadMinimalDto
                {
                    Id = attributeValue.Id,
                    InstrumentId = attributeValue.InstrumentId,
                    AttributeDefinitionId = attributeValue.AttributeDefinitionId,
                    Value = attributeValue.Value
                });
        }

        /// <summary>
        ///     Updates an attribute value of a specific instrument.
        /// </summary>
        /// <remarks>
        ///     Updates a <see cref="InstrumentAttributeValue"/> identified by the provided <paramref name="attributeId"/> associated with
        ///     the <see cref="Instrument"/> corresponding to the provided <paramref name="instrumentId"/>.
        ///     The provided identifiers in the URL must match those in the <paramref name="attribute"/> object, and the associated instrument and
        ///     attribute definition must exist; otherwise a <see cref="BadRequestObjectResult"/> response is returned.
        ///     The newly associated <see cref="AttributeDefinition"/> referenced in <paramref name="attribute"/> must also exist;
        ///     otherwise a <see cref="BadRequestObjectResult"/> response is returned.
        ///     This operation requires admin privileges.
        /// </remarks>
        /// <param name="instrumentId">The unique identifier of the instrument.</param>
        /// <param name="attributeId">The unique identifier of the attribute value.</param>
        /// <param name="attribute">The DTO which holds the data used to update the specific attribute value.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> response if the update was successful;
        ///     otherwise a <see cref="BadRequestObjectResult"/> if the input data is invalid.
        /// </returns>
        [HttpPut("{instrumentId}/attributes/{attributeId}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> UpdateAttributeOfInstrument(
            int instrumentId,
            int attributeId,
            [FromBody] InstrumentAttributeValueUpdateDto attribute
        )
        {
            if (attribute.Id != attributeId)
            {
                return BadRequest(new SimpleMessageDto { Message = "Attribute id mismatch between URL and body." });
            }

            if (attribute.InstrumentId != instrumentId)
            {
                return BadRequest(new SimpleMessageDto { Message = "Instrument id mismatch between URL and body." });
            }

            var instrument = await _dbContext.Instruments.
                Include(i => i.Attributes)
                .ThenInclude(av => av.AttributeDefinition)
                .FirstOrDefaultAsync(i => i.Id == instrumentId);

            if (instrument == null)
            {
                return NotFound(new SimpleMessageDto { Message = "The specified instrument does not exist." });
            }

            var existingAttribute = await _dbContext.InstrumentAttributeValues.FindAsync(attributeId);
            if (existingAttribute == null)
            {
                return NotFound(new SimpleMessageDto { Message = "The specified attribute does not exist." });
            }

            if (existingAttribute.InstrumentId != instrumentId)
            {
                return BadRequest(new SimpleMessageDto
                {
                    Message = "The provided attribute value is not associated with the provided instrument."
                });
            }

            // Check if the new AttributeDefinitionId exists
            var attributeDefinition = await _dbContext.AttributeDefinitions.FindAsync(attribute.AttributeDefinitionId);
            if (attributeDefinition == null)
            {
                return BadRequest(new SimpleMessageDto { Message = "The newly associated attribute definition does not exist." });
            }

            existingAttribute.AttributeDefinitionId = attribute.AttributeDefinitionId;
            existingAttribute.Value = attribute.Value;

            _dbContext.InstrumentAttributeValues.Update(existingAttribute);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        ///     Deletes an existing instrument by its unique identifier.
        /// </summary>
        /// <remarks>
        ///     Deletes an existing <see cref="Instrument"/> corresponding to the provided <paramref name="id"/>.
        ///     The provided identifier must correspond to an existing instrument; otherwise a <see cref="NotFoundResult"/> response is returned.
        ///     This operation requires admin privileges.
        /// </remarks>
        /// <param name="id">The unique identifier of the instrument to be deleted.</param>
        /// <returns>
        ///     A <see cref="NoContentResult"/> if the deletion is successful;
        ///     otherwise a <see cref="NotFoundResult"/> response if no instrument exists with the specified identifier.
        /// </returns>
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

        /// <summary>
        ///     Deletes a specific attribute value from an instrument.
        /// </summary>
        /// <remarks>
        ///     Deletes an <see cref="InstrumentAttributeValue"/> corresponding to the provided <paramref name="attributeId"/> associated with the instrument
        ///     identified by the provided <paramref name="instrumentId"/>.
        ///     Both identifiers must correspond to existing entities; and the targeted <see cref="InstrumentAttributeValue"/> must be associated with the
        ///     targeted <see cref="Instrument"/>; otherwise a <see cref="NotFoundResult"/> response is returned.
        ///     This operation requires admin privileges.
        /// </remarks>
        /// <param name="instrumentId">The unique identifier of the instrument.</param>
        /// <param name="attributeId">The unique identifier of the attribute value.</param>
        /// <returns>
        ///     A <see cref="NoContentResult"/> if the deletion is successful; otherwise a <see cref="NotFoundResult"/> response if input data is invalid.
        /// </returns>
        [HttpDelete("{instrumentId}/attributes/{attributeId}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteAttributeOfInstrument(int instrumentId, int attributeId)
        {
            var instrument = await _dbContext.Instruments
                .Include(i => i.Attributes)
                .FirstOrDefaultAsync(i => i.Id == instrumentId);
            if (instrument == null)
            {
                return NotFound(new SimpleMessageDto { Message = "The specified instrument does not exist." });
            }

            var attributeValue = await _dbContext.InstrumentAttributeValues.FindAsync(attributeId);
            if (attributeValue == null)
            {
                return NotFound(new SimpleMessageDto { Message = "The specified attribute value does not exist." });
            }

            if (attributeValue.InstrumentId != instrumentId)
            {
                return BadRequest(new SimpleMessageDto
                {
                    Message = "The provided attribute value is not associated with the provided instrument."
                });
            }

            instrument.Attributes.Remove(attributeValue);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
