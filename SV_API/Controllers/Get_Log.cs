using sp_api.Data;
using sp_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Get_Log : ControllerBase
    {
        private readonly API_DbContext _context;

        public Get_Log(API_DbContext context)
        {
            _context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdDomain>>> GetAllLogs()
        {
            var result = await _context.LoggingSystem
                .OrderByDescending(x => x.Id)
                .Take(200)
                .ToListAsync();

            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{guid}")]
        public async Task<ActionResult<IEnumerable<AdDomain>>> GetLogsPerGuid(string guid)
        {
            var result = await _context.LoggingSystem
                .Where(x => x.Guid == guid)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(result);
        }
    }
}
