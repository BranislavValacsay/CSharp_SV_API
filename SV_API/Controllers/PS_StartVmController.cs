using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sp_api.Interface;
using System.Threading.Tasks;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PS_StartVmController : ControllerBase
    {
        private IOrchestrator _orchestrator;

        public PS_StartVmController(IOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet("{guid}")]
        public async Task<ActionResult> StartServer(string guid)
        {
            await _orchestrator.StartServer(guid);
            return Ok(new { message = "server started", type = "ok" });
        }
    }
}
