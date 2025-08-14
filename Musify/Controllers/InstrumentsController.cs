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
    }
}
