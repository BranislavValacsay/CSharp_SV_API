using sp_api.Data;
using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;


namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncVmmNetwork : ControllerBase
    {
        public readonly PowerShell_Exec _runMe;
        private readonly API_DbContext _context;
        private readonly ILogJournal _journal;
        private readonly Log _log;

        public SyncVmmNetwork(PowerShell_Exec runMe, API_DbContext context, ILogJournal journal, Log log)
        {
            _runMe = runMe;
            _context = context;
            _journal = journal;
            _log = log;

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet]
        public async Task<IEnumerable<VMMNetwork>?> GetScriptResult()
        {
            string Command = "ps_get_scvmsubnet.ps1";
            var result = await _runMe.StartScript(Command);
            
            List<VMMNetwork> networks = result.FromJson<List<VMMNetwork>>();

            if(networks.IsNullOrEmpty())
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Error in Synchronization: SCVMM networks";
                _log.Result = result;
                return null;
            }

            foreach (VMMNetwork network in networks)
            {
                await GetVmNetwork(network);
            }

            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Synchronization: SCVMM networks";
            _log.Result = result;

            await _journal.SendLog(_log);

            return networks;
            
        }

        private async Task<ActionResult<VMMNetwork>> GetVmNetwork(VMMNetwork network)
        {
            if(_context.VMMNetworks == null)
            {
                return NotFound();
            }
            var result = await _context.VMMNetworks
                .Where(x => x.Name == network.Name)
                .OrderBy(ord => ord.Name)
                .FirstOrDefaultAsync();

            if (result == null)
            {
                await SetVmNetwork(network);
            }
            else
            {
                await ModifyVmNetwork(network);
            }

            return result;
        }
        
        private async Task<ActionResult<VMMNetwork>> SetVmNetwork(VMMNetwork network)
        {
            network.isActive = true;
            _context.VMMNetworks.Add(network);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Ok",network);
        }

        private async Task<ActionResult<VMMNetwork>> ModifyVmNetwork(VMMNetwork network)
        {
            var networkToModify = _context.VMMNetworks.Where(x => x.Name == network.Name).FirstOrDefault();

            _context.Entry(networkToModify).State = EntityState.Modified;
            networkToModify.Gateway = network.Gateway;
            networkToModify.Name = network.Name;
            networkToModify.LogicalNetworkDefinition = network.LogicalNetworkDefinition;
            networkToModify.VlanID = network.VlanID;
            networkToModify.Subnet = network.Subnet;
            networkToModify.Cidr = network.Cidr;            

            await _context.SaveChangesAsync();
            return CreatedAtAction("Modified", networkToModify);
        }

    }




}
