using LARS.Data;
using LARS.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LARS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PS_SetMacAddress : ControllerBase
    {
        private Orchestrator _worker;
        private readonly LarsContext _context;

        public PS_SetMacAddress(Orchestrator worker, LarsContext context)
        {
            _worker = worker;
            _context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet("{guid}")]
        public async Task<ActionResult> SetMacAddress(string guid)
           
        {
            if (guid.IsNullOrEmpty())
            {
                return BadRequest("GUID must not be empty!");
            }

            RequestServer? serverToCreate = await _context.RequestServers
                .Where(x => x.Guid == guid)
                .Include(x => x.VMMNetwork)
                .Include(vmm => vmm.VMMServer)
                .FirstOrDefaultAsync();

            if (serverToCreate == null)
            {
                return NotFound();
            }

            var result = _worker.SetMAC(guid);
            if (result.Result != "Ok")
            {
                return BadRequest("Error during MAC setup, please check");
            }
            return Ok(new { message = "MAC address has been set", type = "ok",result = result.Result });
        }
    }
}
