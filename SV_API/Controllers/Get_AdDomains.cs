using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sp_api.Data;
using sp_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Get_AdDomains : ControllerBase
    {
        private readonly API_DbContext _context;

        public Get_AdDomains(API_DbContext context)
        {
            _context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdDomain>>> GetDomains()
        {
            var result = await _context.AdDomains.ToListAsync();
            return Ok(result);
        }
    }
}
