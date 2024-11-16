using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Db;
using Back.Models;

namespace Back.Controllers
{
    [Route("leigraci")]
    [ApiController]
    public class LeIgraciController : ControllerBase
    {
        private readonly MySqlContext _db;

        public LeIgraciController(MySqlContext context)
        {
            _db = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeIgraci>>> GetLeIgracis()
        {
            return await _db.LeIgracis.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeIgraci>> GetLeIgraci(int id)
        {
            var leIgraci = await _db.LeIgracis.FindAsync(id);

            if (leIgraci == null)
            {
                return NotFound("Igrac nije pronaden");
            }

            return leIgraci;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLeIgraci(int id, LeIgraci leIgraci)
        {
            var player = _db.LeIgracis.Where(x => x.Id == id).First();
            
            player.Name = leIgraci.Name;
            player.Number = leIgraci.Number;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeIgraciExists(id))
                {
                    return NotFound("Igrac nije pronaden");
                }
                else
                {
                    throw;
                }
            }

            return Ok($"Igrac {id} edited");
        }

        [HttpPost]
        public async Task<ActionResult<LeIgraci>> PostLeIgraci(LeIgraci leIgraci)
        {
            _db.LeIgracis.Add(leIgraci);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LeIgraciExists(leIgraci.Id))
                {
                    return Conflict("Problem s bazom");
                }
                else
                {
                    throw;
                }
            }

            return Ok($"Added {leIgraci.Name} to db");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeIgraci(int id)
        {
            var leIgraci = await _db.LeIgracis.FindAsync(id);
            if (leIgraci == null)
            {
                return BadRequest("Igrac nije pronaden");
            }

            _db.LeIgracis.Remove(leIgraci);
            await _db.SaveChangesAsync();

            return Ok($"Igrac {id} izbrisan");
        }

        private bool LeIgraciExists(int id)
        {
            return _db.LeIgracis.Any(e => e.Id == id);
        }
    }
}
