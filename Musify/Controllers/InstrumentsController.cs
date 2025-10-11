using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
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
                return BadRequest("Associated category does not exist.");
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
                return BadRequest("Instrument id is invalid.");
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
