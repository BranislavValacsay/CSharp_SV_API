using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using sp_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PS_CreateDSC : ControllerBase
    {
        private readonly IOrchestrator _orchestrator;

        public PS_CreateDSC(IOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpPost("{guid}")]
        public async Task<ActionResult> CreateDscDocuments(string guid)
        {
            await _orchestrator.CreateDSC(guid);

            return Ok(new { message = "DSC creation triggered", type = "ok" });
        }

    }
}

