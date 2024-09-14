using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sp_api.Data;
using sp_api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VmmNetworks : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly API_DbContext _context;

        public VmmNetworks(IMapper mapper, API_DbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VMMNetworkDTO>>> GetNetworks([FromQuery] FilterParams filterParams)
        {
            if (_context.VMMNetworks == null)
            {
                return NotFound();
            }
            var result = _context.VMMNetworks
                .OrderBy(ord => ord.VlanID);
            if (filterParams.Key != null)
            {
                result = (IOrderedQueryable<VMMNetwork>)result.Where(
                    x => x.Name.Contains(filterParams.Key) ||
                    x.LogicalNetworkDefinition.Contains(filterParams.Key) ||
                    x.VlanID.ToString().Contains(filterParams.Key) ||
                    x.Subnet.Contains(filterParams.Key) ||
                    x.Cidr.ToString().Contains(filterParams.Key) ||
                    x.Gateway.Contains(filterParams.Key)
                );
            }
            var convertedResult = await result.ToListAsync();
            List<VMMNetworkDTO> resultDTO = _mapper.Map<List<VMMNetworkDTO>>(convertedResult);

            return resultDTO;
        }
    }
}
