using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
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
        public async Task<ActionResult<Instrument>> CreateInstrument([FromBody] Instrument instrument)
        {
            if (instrument == null)
            {
                return BadRequest("Instrument cannot be null.");
            }

            _dbContext.Instruments.Add(instrument);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInstrumentById), new { id = instrument.Id }, instrument);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<ActionResult<Instrument>> UpdateInstrument(int id, [FromBody] Instrument instrument)
        {
            if (instrument == null || instrument.Id != id)
            {
                return BadRequest("Instrument data is invalid.");
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
