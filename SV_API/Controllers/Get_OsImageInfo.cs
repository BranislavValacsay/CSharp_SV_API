using AutoMapper;
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
    public class Get_OsImageInfo : ControllerBase
    {
        private readonly API_DbContext _context;
        private readonly IMapper _mapper;

        public Get_OsImageInfo(API_DbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WindowsVersion>>>GetWindowsVersion()
            {
                if(_context.WindowsVersions == null)
            {
                return NotFound();
            }
                return await _context.WindowsVersions.OrderBy(ord => ord.Id).Reverse().ToListAsync();
            }
    }
}
