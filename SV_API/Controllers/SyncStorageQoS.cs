using sp_api.Data;
using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncStorageQoS : ControllerBase
    {
        public readonly PowerShell_Exec _runMe;
        private readonly API_DbContext _context;
        private readonly ILogJournal _journal;
        private readonly Log _log;

        public SyncStorageQoS(PowerShell_Exec runMe, API_DbContext context, ILogJournal journal, Log log)
        {
            _runMe = runMe;
            _context = context;
            _journal = journal;
            _log = log;

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet]
        public async Task<IEnumerable<StorageQoSPolicies>> SyncStorageQos()
        {
            string Command = "ps_get_scvmm_storageQoS.ps1";
            var result = await _runMe.StartScript(Command);

            List<StorageQoSPolicies> storageQosPolicies = result.FromJson<List<StorageQoSPolicies>>();

            foreach(StorageQoSPolicies storageQosPolicy in storageQosPolicies)
            {
                if ((storageQosPolicy.name).Contains("SQL"))
                {
                    storageQosPolicy.isSql = true;
                }
                if ((storageQosPolicy.name).Contains("Prod"))
                {
                    storageQosPolicy.classification = "prod";
                }

                await GetStorageQosPolicies(storageQosPolicy);

                _log.MessageType = MessageType.Message;
                _log.MessageBody = "Synchronization: storage QoS policies";
                _log.Result = result;

                await _journal.SendLog(_log);
            }

            return storageQosPolicies;

        }

        private async Task<ActionResult<StorageQoSPolicies>> GetStorageQosPolicies(StorageQoSPolicies policy)
        {
            if (_context.StorageQoSPolicies == null) 
            {
                return NotFound();
            }
            var result = await _context.StorageQoSPolicies
                .Where(x => x.name == policy.name)
                .OrderBy(ord => ord.name)
                .FirstOrDefaultAsync();

            if (result == null)
            {
                await SetStorageQosPolicies(policy);
            }

            return result;
        }
        private async Task<ActionResult<StorageQoSPolicies>> SetStorageQosPolicies(StorageQoSPolicies policy)
        {
            _context.StorageQoSPolicies.Add(policy);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Ok", policy);
        }
    }
}
