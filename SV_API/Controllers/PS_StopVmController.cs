using sp_api.Data;
using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Text.Json;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PS_StopVmController : ControllerBase
    {
        public readonly PowerShell_Exec _runMe;
        private readonly API_DbContext _context;
        private readonly ILogJournal _journal;
        private readonly Log _log;

        public PS_StopVmController(PowerShell_Exec runMe, API_DbContext context, ILogJournal journal, Log log)
        {
            _runMe = runMe;
            _context = context;
            _journal = journal;
            _log = log;

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet("{guid}")]
        public async Task<ActionResult> pass(string guid)
        {
            RequestServer? serverToCreate = await _context.RequestServers
                .Where(x => x.Guid == guid)
                .Include(x => x.VMMNetwork)
                .Include(vmm => vmm.VMMServer)
                .FirstOrDefaultAsync();

            if (serverToCreate == null)
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Power: There is no server found in database to process.";
                _log.Guid = guid;
                await _journal.SendLog(_log);

                return NotFound();
            }

            string serverProps = ".\\ps_get_vServer_properties.ps1";
            string startScriptArguments = " -vmname " + serverToCreate.ServerName;
            startScriptArguments += " -vmmserver '" + serverToCreate.VMMServer.Name + "'";
            serverProps += startScriptArguments;

            var ps_serverResult = await _runMe.StartScript(serverProps);

            vServerProperties convertedStatusResult = ps_serverResult.FromJson<vServerProperties>();

            if (convertedStatusResult.virtualMachineState == "PowerOff")
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Power: server " + serverToCreate.ServerName + "is already in state PowerOff";
                _log.Guid = guid;
                await _journal.SendLog(_log);

                return BadRequest(new { error = "vServer is already in state PowerOff" });
            }

            string script = ".\\ps_stop_vm.ps1";
            string arguments = " -vmName '" + serverToCreate.ServerName + "'";
            arguments += " -vmmserver '" + serverToCreate.VMMServer.Name + "'";
            script += arguments;

            var result = _runMe.StartScript(script);

            _log.Command = script;
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Power: server " + serverToCreate.ServerName + " shut down.";
            _log.Result = JsonSerializer.Serialize(result);
            _log.Guid = guid;
            await _journal.SendLog(_log);

            return Ok(new { message = "server stopped", type = "ok", command = script, result = result });
        }
    }
}
