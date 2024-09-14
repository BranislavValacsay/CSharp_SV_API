using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetIPAddressStatus : ControllerBase
    {
        
        public readonly PowerShell_Exec _runMe;
        private readonly ILogJournal _journal;
        private readonly Log _log;

        public SetIPAddressStatus(PowerShell_Exec runMe, ILogJournal journal, Log log)
        {
            _runMe = runMe;
            _journal = journal;
            _log = log;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet]
        public async Task<string> GetScriptResult([FromQuery] IpStatus IPStatusParams)
        {

            string ipaddress = IPStatusParams.ipaddress;
            string hostname = IPStatusParams.hostname;
            string status = IPStatusParams.status;
            string guid = IPStatusParams.guid;

            string script = "ps_set_ipplan_status.ps1";
            
            string arguments = " -ipaddress '" + ipaddress + "'";
            arguments += " -hostname '" + hostname + "'";
            arguments += " -status '" + status + "'";

            script += arguments;
            
            var result = await _runMe.StartScript(script);

            
            _log.Command = script;
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request: IP address set to : '" + status + "'";
            _log.Result = result;
            _log.Guid = guid;
            
            await _journal.SendLog(_log);
            return result;
        }
    }
}
