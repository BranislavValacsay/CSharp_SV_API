using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using sp_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PS_renameSystemDisk : ControllerBase
    {
        private IOrchestrator _orchestrator;

        public PS_renameSystemDisk(IOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpPost("{guid}")]
        public async Task<ActionResult> RenameSystemDisk(string guid)
        {
            await _orchestrator.RenameSystemDisk(guid);

            return Ok(new { message = "disk has been renamed successfully", type = "ok" });
        }
    }
}
