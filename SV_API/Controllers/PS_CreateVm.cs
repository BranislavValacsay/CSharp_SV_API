using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using sp_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PS_CreateVm : ControllerBase
    {
        private IOrchestrator _orchestrator;

        public PS_CreateVm(IOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpPost("{guid}")]
        public async Task<ActionResult> CreateVmServer(string guid)
        {
            await _orchestrator.CreateVm(guid);
            return Ok(new { message = "Server provisioning started", type = "ok" });
        }

    }
}
