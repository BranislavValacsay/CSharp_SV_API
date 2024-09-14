using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LARS.Data;
using System.Linq;
using LARS.Models;

namespace LARS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerNamesController : ControllerBase
    {
        private readonly LarsContext _context;

        public ServerNamesController(LarsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServerName>>> GetServerNameTable()
        {
          if (_context.ServerNameTable == null)
          {
                NotFound();
          }
            var result = await _context.ServerNameTable.ToListAsync();
            if(result == null) {
                return NotFound();
            }
            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServerName>> GetServerName(int id)
        {
          if (_context.ServerNameTable == null)
          {
              return NotFound();
          }
            var serverName = await _context.ServerNameTable.FindAsync(id);

            if (serverName == null)
            {
                return NotFound();
            }

            return serverName;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutServerName(int id, ServerName serverName)
        {
            if (id != serverName.Id)
            {
                return BadRequest();
            }

            _context.Entry(serverName).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServerNameExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpPost]
        public async Task<ActionResult<ServerName>> PostServerName(ServerName serverName)
        {
          if (_context.ServerNameTable == null)
          {
              return Problem("Entity set 'LarsContext.ServerNameTable'  is null.");
          }
            _context.ServerNameTable.Add(serverName);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetServerName", new { id = serverName.Id }, serverName);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServerName(int id)
        {
            if (_context.ServerNameTable == null)
            {
                return NotFound();
            }
            var serverName = await _context.ServerNameTable.FindAsync(id);
            if (serverName == null)
            {
                return NotFound();
            }

            _context.ServerNameTable.Remove(serverName);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServerNameExists(int id)
        {
            return (_context.ServerNameTable?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
