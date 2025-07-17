using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerfumeAPI.Data;
using PerfumeAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerfumeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerfumeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PerfumeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Perfume>>> GetPerfumes()
        {
            return await _context.Perfumes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Perfume>> GetPerfume(int id)
        {
            var perfume = await _context.Perfumes.FindAsync(id);
            if (perfume == null)
            {
                return NotFound();
            }
            return perfume;
        }

        [HttpPost]
        public async Task<ActionResult<Perfume>> PostPerfume(Perfume perfume)
        {
            _context.Perfumes.Add(perfume);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPerfume), new { id = perfume.Id }, perfume);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerfume(int id, Perfume perfume)
        {
            if (id != perfume.Id)
            {
                return BadRequest();
            }

            _context.Entry(perfume).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerfume(int id)
        {
            var perfume = await _context.Perfumes.FindAsync(id);
            if (perfume == null)
            {
                return NotFound();
            }

            _context.Perfumes.Remove(perfume);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
