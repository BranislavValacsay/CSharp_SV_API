using sp_api.Data;
using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace sp_api.Services
{
    public class Orchestrator : IOrchestrator
    {
        public readonly PowerShell_Exec _runPowerShell;
        private readonly API_DbContext _context;
        private ILogJournal _journal;

        public Orchestrator(PowerShell_Exec runPowerShell, API_DbContext context, ILogJournal journal)
        {
            _runPowerShell = runPowerShell;
            _context = context;
            _journal = journal;
        }
        public async Task MainOrchestrationLoop()
        {
            string guid = await getRequestsToProcess();
            Log _log = new();
            if (!guid.IsNullOrEmpty())
            {
                int status = await getRequest_StatusOnly(guid);

                switch (status)
                {
                    case 10:
                        _log = SetLog(guid, "Stage 1: request found, ", MessageType.Message, "");
                        await _journal.SendLog(_log);

                        _log = SetLog(guid, "Getting server name from automation", MessageType.Message, "");
                        await _journal.SendLog(_log);
                        await GetNameForServer(guid);

                        _log = SetLog(guid, "Processing IP address", MessageType.Message, "");
                        await _journal.SendLog(_log);
                        await GetFirstFreeIp(guid);

                        _log = SetLog(guid, "Checking if request has NAME and IP address set. If yes, moving to next stage.", MessageType.Message, "");
                        await _journal.SendLog(_log);
                        await CheckRequest_IP_NAME(guid);

                        break;

                    case 20:
                        string checkIfExist = await getVirtualServerStatus(guid);
                        _log = SetLog(guid, "Stage 2: virtual server creation", MessageType.Message, status.ToString());
                        await _journal.SendLog(_log);
                        await RequestStatusChange(guid, 40);
                        try
                        {
                            Response_ServerStatus jsonResponse = JsonSerializer.Deserialize<Response_ServerStatus>(checkIfExist);
                            if (!jsonResponse.name.IsNullOrEmpty())
                            {
                                _log = SetLog(guid, "Stage 2: Server has been found existing on VMM. Please review the request.", MessageType.Error, status.ToString());
                                await _journal.SendLog(_log);
                                await RequestStatusChange(guid, -20);
                                break;
                            }
                        }
                        catch
                        {
                            _log = SetLog(guid, "Stage 2: Unknown error has occurred. Please review the request.", MessageType.Error, status.ToString());
                            await _journal.SendLog(_log);
                            RequestStatusChange(guid, -21);
                            break;
                        }
                        await CreateVm(guid);
                        break;
                    case 30:
                        break;
                    case 40:
                        string virtualMachineStatus = await getVirtualServerStatus(guid);
                        Response_ServerStatus jsonServerStatus = JsonSerializer.Deserialize<Response_ServerStatus>(virtualMachineStatus);

                        if (jsonServerStatus.status == "PowerOff")
                        {
                            _log = SetLog(guid, "Stage 4: Server has been created and it is in status 'PowerOff', proceeding", MessageType.Message, status.ToString());
                            await _journal.SendLog(_log);
                            await RequestStatusChange(guid, 50);
                        }
                        else
                        {
                            _log = SetLog(guid, "Stage 4: Server is not in status 'PowerOff', it is in status: '" + jsonServerStatus.status + "'", MessageType.Warning, status.ToString());
                            await _journal.SendLog(_log);

                            if (jsonServerStatus.status == "NotFound")
                            {
                                await RequestStatusChange(guid, -20);
                                _log = SetLog(guid, "Stage 4: Server was not found at all, but it should be present at this stage either in 'PowerOff' or 'UnderCreation'", MessageType.Error, "Status:" + status.ToString());
                            }
                        }
                        break;
                    case 50:
                        _log = SetLog(guid, "Stage 4: creating DSC configuration", MessageType.Message, status.ToString());
                        await _journal.SendLog(_log);
                        await CreateDSC(guid);
                        await RequestStatusChange(guid, 60);
                        break;
                    case 60:
                        _log = SetLog(guid, "Stage 5: Injecting DSC configuration to system drive of new server", MessageType.Message, status.ToString());
                        await _journal.SendLog(_log);
                        await InjectDSC(guid);
                        await RequestStatusChange(guid, 70);
                        break;
                    case 70:
                        _log = SetLog(guid, "Stage 6: System disk rename", MessageType.Message, status.ToString());
                        await _journal.SendLog(_log);
                        await RenameSystemDisk(guid);
                        await RequestStatusChange(guid, 80);
                        break;
                    case 80:
                        _log = SetLog(guid, "Stage 7: Starting server", MessageType.Message, status.ToString());
                        await _journal.SendLog(_log);
                        await StartServer(guid);
                        await RequestStatusChange(guid, 90);
                        break;
                    case 90:
                        _log = SetLog(guid, "Stage 8: Finalizing request.", MessageType.Message, status.ToString());
                        await _journal.SendLog(_log);
                        await MarkRequestComplete(guid);
                        await RequestStatusChange(guid, 100);
                        break;
                    default:
                        break;
                }
            }
        }
        private async Task test(string guid)
        {
            string temp = await getVirtualServerStatus(guid);
            var jsn = JsonSerializer.Deserialize<Response_ServerStatus>(temp);
            string path = @"c:\temp\time.txt";

            RequestServer server = await getRequest_Details(guid);
            using (StreamWriter writer = new StreamWriter(path, true))

            {
                writer.WriteLine(DateTime.Now.ToString() + " " + jsn.status + " " + server.ServerName + " " + server.Status);
            }
        }
        public async Task CreateVm(string guid)
        {
            Log _log = new();
            RequestServer? serverToCreate = await getRequest_Details(guid);

            if (serverToCreate == null)
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Request: no server to create found. Please check settings";
                _log.Guid = guid;
                await _journal.SendLog(_log);
                await RequestStatusChange(guid, -20);
            }

            Random random = new Random();
            string vServerDescription = serverToCreate.BlimpName + " | " + serverToCreate.Guid + " | " + "schedule_0" + random.Next(1, 6);

            var classification = "test";
            if (serverToCreate.Domain != "faketestdomain.test") { classification = "prod"; }

            var storageQoSPolicy = await _context.StorageQoSPolicies
                .Where(x => x.allowed == true)
                .Where(s => s.isSql == serverToCreate.IsSQLServer)
                .Where(c => c.classification == classification)
                .FirstOrDefaultAsync();

            string storageQoSPolicyString = storageQoSPolicy.name;
            if (storageQoSPolicy.name == null) { storageQoSPolicyString = "VM_Prod_Standard"; }

            try
            {
                string script = ".\\ps_provisioning.ps1";
                string arguments = " -vmName '" + serverToCreate.ServerName + "'";
                arguments += " -vmmServer '" + serverToCreate.VMMServer.Name + "'";
                arguments += " -guid " + serverToCreate.Guid;
                arguments += " -cpu " + serverToCreate.CPU;
                arguments += " -ram " + serverToCreate.Memory * 1024;
                arguments += " -image '" + serverToCreate.WindowsVersion.ImageName + "'";
                arguments += " -osVersion " + ReturnOsVersion(serverToCreate.WindowsVersion.Name);
                arguments += " -storage_QosPolicy '" + storageQoSPolicyString + "'";
                arguments += " -network '" + serverToCreate.VMMNetwork.Name + "'";
                arguments += " -vmDescription '" + vServerDescription + "'";
                arguments += " -isSQl '" + serverToCreate.IsSQLServer + "'";
                arguments += " -blimpName '" + serverToCreate.BlimpName + "'";
                arguments += " -isInfra '" + serverToCreate.IsInfraServer + "'";
                arguments += " -osName '" + serverToCreate.WindowsVersion.Name + "'";
                arguments += " -disk_D '" + serverToCreate.Disk_D + "'";
                arguments += " -disk_E '" + serverToCreate.Disk_E + "'";
                arguments += " -domain '" + serverToCreate.Domain + "'";
                script += arguments;

                var result = _runPowerShell.StartScript(script);

                _log.Command = script;
                _log.MessageType = MessageType.Message;
                _log.MessageBody = "Request: virtual machine creation";
                _log.Result = result.ToString();
                _log.Guid = guid;
                await _journal.SendLog(_log);
                await RequestStatusChange(guid, 40);
            }
            catch
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Request: virtual machine creation";
                _log.Guid = guid;
                await RequestStatusChange(guid, -10);
            }
        }
        public async Task<string> GetNameForServer(string guid) // does not have _log
        {
            RequestServer? request = await getRequest_Details(guid);
            if (request == null)
            {
                await RequestStatusChange(guid, -10);
            }

            if (!request.ServerName.IsNullOrEmpty())
            {
                ServerName returnName = new ServerName();
                returnName.Id = 0;

                string temp = Regex.Replace(request.ServerName, @"\D", "");

                returnName.Name = Convert.ToInt16(temp);
                var classification = "p";
                if (request.Domain == "faketestdomain.test") classification = "t";

                var serverName = "prodserver" + returnName.Name + classification;
                request.ServerName = serverName;

                _context.Entry(request).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return returnName.Name.ToString();
            }

            else
            {
                string highestServerName = _context.RequestServers.Where(a => a.ServerName != "").Max(x => x.ServerName) ?? "";
                int highestServerValue = Convert.ToInt16(Regex.Replace(highestServerName, @"\D", ""));

                int highestServerNameReservation = _context.ServerNameTable.Max(x => (int?)x.Name) ?? 0;


                ServerName returnName = new ServerName();
                returnName.Id = 0;
                returnName.Name = CompareNumbers(highestServerValue, highestServerNameReservation);

                returnName.Name++;

                var classification = "p";
                if (request.Domain == "faketestdomain.test") classification = "t";

                var serverName = "prodserver" + returnName.Name + classification;
                request.ServerName = serverName;

                _context.Entry(request).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return returnName.Name.ToString();
            }
        }
        public async Task<Response_Ipplan> GetFirstFreeIp(string guid)
        {
            RequestServer request = await getRequest_Details(guid);
            Log _log = new();
            if (request.IPAddress.IsNullOrEmpty())
            {
                string network = request.VMMNetwork.Subnet;
                int mask = request.VMMNetwork.Cidr;
                string script = "ps_get_first_ip_ipplan.ps1";

                string arguments = " -network '" + network + "'";
                arguments += " -mask '" + mask + "'";

                script += arguments;

                var result = await _runPowerShell.StartScript(script);

                Response_Ipplan ipplanResponse = new();

                try
                {
                    ipplanResponse = JsonSerializer.Deserialize<Response_Ipplan>(result);
                }
                catch (Exception ex)
                {
                    _log.MessageType = MessageType.Error;
                    _log.MessageBody = "Request: IP address - error during operation. Please check the script.";
                    _log.Result = result;
                    _log.Guid = guid;

                    await _journal.SendLog(_log);
                    await RequestStatusChange(guid, -10);
                    return await Task.FromException<Response_Ipplan>(ex);
                }
                if (!ipplanResponse.IPAddress.IsNullOrEmpty())
                {
                    _log.Command = script;
                    _log.MessageType = MessageType.Message;
                    _log.MessageBody = "Request: IP address";
                    _log.Result = result;
                    _log.Guid = guid;

                    request.IPAddress = ipplanResponse.IPAddress;
                    _context.Entry(request).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                    await _journal.SendLog(_log);
                }
                else
                {
                    _log.MessageType = MessageType.Error;
                    _log.MessageBody = "Request: IP address - not found. Verify if subnet is full or pick IP address manually";
                    _log.Result = result;
                    _log.Guid = guid;

                    await _journal.SendLog(_log);
                    await RequestStatusChange(guid, -10);

                    var exception = new TaskCompletionSource<Response_Ipplan>();
                    exception.SetException(new Exception(_log.MessageBody));
                    return await exception.Task;
                }
                await ReserveIpAddress(guid);
                return ipplanResponse;
            }
            Response_Ipplan response = new Response_Ipplan
            {
                Network = request.VMMNetwork.Subnet,
                Cidr = request.VMMNetwork.Cidr,
                IPAddress = request.IPAddress
            };
            return response;
        }
        public async Task CreateDSC(string guid)
        {
            RequestServer? serverToCreate = await getRequest_Details(guid);
            Log _log = new();
            if (serverToCreate == null)
            {
                return;
            }
            if (serverToCreate.ServerName.IsNullOrEmpty())
            {
                return;
            }
            if (serverToCreate.NetworkId.IsNullOrEmpty())
            {
                return;
            }
            string script = ".\\ps_create_dsc_config.ps1";
            string arguments = " -vmName '" + serverToCreate.ServerName + "'";
            arguments += " -Domain '" + serverToCreate.Domain + "'";
            arguments += " -IPAddress '" + serverToCreate.IPAddress + "'";
            arguments += " -cidr " + serverToCreate.VMMNetwork.Cidr;
            arguments += " -Gateway '" + serverToCreate.VMMNetwork.Gateway + "'";
            arguments += " -serverToFinishBlimp_Id '" + serverToCreate.BlimpId + "'";
            arguments += " -serverToFinishBlimp_Env '" + serverToCreate.BlimpEnv + "'";
            arguments += " -disk_D " + serverToCreate.Disk_D;
            arguments += " -disk_E " + serverToCreate.Disk_E;
            script += arguments;
            var result = await _runPowerShell.StartScript(script);
            _log.Command = script;
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request: Creating DSC configuration";
            _log.Result = result;
            _log.Guid = guid;
            await _journal.SendLog(_log);
            await RequestStatusChange(guid, 50);
        }
        public async Task InjectDSC(string guid)
        {
            RequestServer? serverToCreate = await getRequest_Details(guid);
            Log _log = new();
            if (serverToCreate == null)
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Request: DSC injection. There are no server found in database to process.";
                _log.Guid = guid;
                await _journal.SendLog(_log);
                return;
            }
            if (serverToCreate.ServerName.IsNullOrEmpty())
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Request: DSC injection. There is no server found in database to process.";
                _log.Guid = guid;
                await _journal.SendLog(_log);
                return;
            }
            if (serverToCreate.NetworkId.IsNullOrEmpty())
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Request: DSC injection. Network is empty in request. Please fill network.";
                _log.Guid = guid;
                await _journal.SendLog(_log);
                return;
            }
            string script = ".\\ps_inject_dsc.ps1";
            string arguments = " -vmname '" + serverToCreate.ServerName + "'";
            arguments += " -vmmserver '" + serverToCreate.VMMServer.Name + "'";
            script += arguments;
            var result = _runPowerShell.StartScript(script);
            _log.Command = script;
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request: DSC injection";
            _log.Result = JsonSerializer.Serialize(result);
            _log.Guid = guid;
            await _journal.SendLog(_log);
            await RequestStatusChange(guid, 60);
        }
        public async Task RenameSystemDisk(string guid)
        {
            RequestServer serverToCreate = await getRequest_Details(guid);
            Log _log = new();
            if (serverToCreate == null)
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Request: system disk rename. There is no server found in database to process.";
                _log.Guid = guid;
                await _journal.SendLog(_log);
                return;
            }
            if (serverToCreate.ServerName.IsNullOrEmpty())
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Request: system disk rename. There is no server found in database to process.";
                _log.Guid = guid;
                await _journal.SendLog(_log);
                return;
            }
            string script = ".\\ps_rename_systemDisk.ps1";
            string arguments = " -vmName '" + serverToCreate.ServerName + "'";
            arguments += " -vmmserver '" + serverToCreate.VMMServer.Name + "'";
            script += arguments;
            var result = await _runPowerShell.StartScript(script);
            _log.Command = script;
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request: system disk rename";
            _log.Result = result;
            _log.Guid = guid;
            await _journal.SendLog(_log);
            await RequestStatusChange(guid, 70);
            return;
        }
        public async Task StartServer(string guid)
        {
            RequestServer? serverToCreate = await getRequest_Details(guid);
            Log _log = new();
            if (serverToCreate == null)
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Power: There is no server found in database to process.";
                _log.Guid = guid;
                await _journal.SendLog(_log);
                return;
            }
            string script = ".\\ps_start_vm.ps1";
            string arguments = " -vmName '" + serverToCreate.ServerName + "'";
            arguments += " -vmmserver '" + serverToCreate.VMMServer.Name + "'";
            script += arguments;
            var result = _runPowerShell.StartScript(script);
            _log.Command = script;
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Power: server " + serverToCreate.ServerName + " started";
            _log.Result = JsonSerializer.Serialize(result);
            _log.Guid = guid;
            await _journal.SendLog(_log);
            await RequestStatusChange(guid, 90);
        }
        public async Task<string> UpdateLeonDb(string guid) // does not have _log
        {
            RequestServer? serverToCreate = await _context.RequestServers
                            .Where(x => x.Guid == guid)
                            .FirstOrDefaultAsync();
            if (serverToCreate == null)
            {
                return "NotFound";
            }
            string serverProps = ".\\ps_Update_LeonDB.ps1";
            string startScriptArguments = " -vmname " + serverToCreate.ServerName;
            startScriptArguments += " -leonrequestId '" + serverToCreate.LeonRequestId + "'";
            serverProps += startScriptArguments;
            var result = await _runPowerShell.StartScript(serverProps);
            return result;
        }

        private async Task<string> getRequestsToProcess()
        {
            string? result = await _context.RequestServers
                .Where(x => x.Status >= 10)
                .Where(y => y.Status < 100)
                .Where(z => z.ManualOverride != true)
                .Select(z => z.Guid)
                .FirstOrDefaultAsync();
            return result;
        }
        private async Task<RequestServer> getRequest_Details(string guid)
        {
            return await _context.RequestServers
                .Where(x => x.Guid == guid)
                .Include(x => x.VMMNetwork)
                .Include(x => x.WindowsVersion)
                .Include(x => x.VMMServer).FirstOrDefaultAsync();
        }
        public async Task RequestStatusChange(string guid, int status)
        {
            RequestServer request = await _context.RequestServers.Where(x => x.Guid == guid).FirstOrDefaultAsync();
            request.Status = status;
            if (request.Guid.IsNullOrEmpty()) { throw new Exception("Error ! No server with given guid was found:" + guid); }
            _context.RequestServers.Entry(request).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        private int ReturnOsVersion(string strOsVersion)
        {
            int OsVersion = Convert.ToInt16(Regex.Replace(strOsVersion, @"\D", ""));
            return OsVersion;
        }
        private async Task<int> getRequest_StatusOnly(string guid)
        {
            return await _context.RequestServers
                .Where(x => x.Guid == guid)
                .Select(a => a.Status)
                .FirstOrDefaultAsync();
        }
        private async Task<string> getVirtualServerStatus(string guid)
        {
            Log _log = new();
            RequestServer server = await getRequest_Details(guid);
            string vmname = server.ServerName;
            string vmmserver = server.VMMServer.Name;
            string script = "ps_get_vServer_properties.ps1";
            string arguments = " -vmname \"" + vmname + "\" -vmmserver \"" + vmmserver + "\"";
            script += arguments;
            var result = await _runPowerShell.StartScript(script);
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Getting server status from VMM, result: '" + result + "'";
            _log.Guid = guid;
            _log.Command = script;
            await _journal.SendLog(_log);
            return result;
        }
        private async Task ReserveIpAddress(string guid)
        {
            Log _log = new();
            RequestServer request = await getRequest_Details(guid);
            string ipaddress = request.IPAddress;
            string hostname = request.ServerName + "." + request.Domain;
            string status = "Used";
            string script = "ps_set_ipplan_status.ps1";
            string arguments = " -ipaddress '" + ipaddress + "'";
            arguments += " -hostname '" + hostname + "'";
            arguments += " -status '" + status + "'";
            script += arguments;
            var result = await _runPowerShell.StartScript(script);
            _log.Command = script;
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request: IP address set to USED";
            _log.Result = result;
            _log.Guid = guid;
            await _journal.SendLog(_log);
        }
        private int CompareNumbers(int a, int b)
        {
            return Math.Max(a, b);
        }
        private async Task CheckRequest_IP_NAME(string guid)
        {
            Log _log = new();
            RequestServer request = await getRequest_Details(guid);
            if (!request.ServerName.IsNullOrEmpty() && !request.IPAddress.IsNullOrEmpty())
            {
                _log = SetLog(guid, "Server name and IP address set, moving to next stage", MessageType.Message, request.Status.ToString());
                await _journal.SendLog(_log);
                await ReserveIpAddress(guid);
                await RequestStatusChange(guid, request.Status + 10);
            }
            else
            {
                _log = SetLog(guid, "Server name and IP address missing, waiting for information to be provided", MessageType.Message, request.Status.ToString());
                await _journal.SendLog(_log);
            }
        }
        private async Task MarkRequestComplete(string guid)
        {
            Log _log = new();
            RequestServer request = await getRequest_Details(guid);
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request for server " + request.ServerName + " marked as complete.";
            _log.Guid = guid;
            await _journal.SendLog(_log);
            await RequestStatusChange(guid, 100);
        }
        private Log SetLog(string guid, string messageBody, MessageType type, string result)
        {
            Log _log = new();
            _log.Guid = guid;
            _log.MessageBody = messageBody;
            _log.MessageType = type;
            _log.Result = result;
            return _log;
        }
    }
}
