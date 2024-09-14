using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Get_FirstFreeIp : ControllerBase
    {
        public readonly PowerShell_Exec _runMe;
        private readonly ILogJournal _journal;

        public Get_FirstFreeIp(PowerShell_Exec runMe, ILogJournal journal)
        {
            _runMe = runMe;
            _journal = journal;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet]
        public async Task<string> GetScriptResult([FromQuery] IPPlanParams IPPlanParams)
        {
            Log _log = new();
            string network = IPPlanParams.Network;
            int mask = IPPlanParams.Mask;
            string guid = IPPlanParams.Guid;
            string script = "ps_get_first_ip_ipplan.ps1";
            string arguments = " -network '" + network + "'";
            arguments += " -mask '" + mask + "'";
            script += arguments;
            var result = await _runMe.StartScript(script);
            _log.Command = script;
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request: IP address";
            _log.Result = result;
            _log.Guid = guid;
            await _journal.SendLog(_log);
            return result;
        }
    }
}
