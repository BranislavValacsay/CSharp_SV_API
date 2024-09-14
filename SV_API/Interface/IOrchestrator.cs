using sp_api.Data;
using sp_api.Models;

namespace sp_api.Interface
{
    public interface IOrchestrator
    {
        Task MainOrchestrationLoop();
        Task CreateVm(string guid);
        Task CreateDSC(string guid);
        Task InjectDSC(string guid);
        Task RenameSystemDisk(string guid);
        Task StartServer (string guid);
        Task<string> UpdateLeonDb(string guid);
        Task RequestStatusChange(string guid, int status);
        Task<Response_Ipplan> GetFirstFreeIp(string guid);
        Task<string> GetNameForServer(string guid);
    }
}