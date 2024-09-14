using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using sp_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PS_InjectDsc : ControllerBase
    {
        private IOrchestrator _orchestrator;

        public PS_InjectDsc(IOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpPost("{guid}")]
        public async Task<ActionResult> pass(string guid)
        {

            await _orchestrator.InjectDSC(guid);

            return Ok(new { message = "dsc configuration deployed", type = "ok" });
        }
    }
}
