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
    public class Get_Locations_VmmServers : ControllerBase
    {
        private readonly API_DbContext _context;
        private readonly IMapper _mapper;

        public Get_Locations_VmmServers(API_DbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VmmServerDTO>>> GetLocationsVmmServers()
        {
            var result = await _context.VMMServers
                .Include(x => x.Location)
                .Select(location => new VmmServerDTO
                {
                    Id = location.Id,
                    Name = location.Name,
                    Location = location.Location.Name
                })
                .ToListAsync();
           
            return Ok(result);
        }
    }
}
