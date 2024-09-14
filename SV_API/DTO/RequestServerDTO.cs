using sp_api.Models;
using System.Security.Policy;

namespace sp_api.DTO
{
    public class RequestServerDTO
    {
        public string? Guid { get; set; }
        public int CPU { get; set; }
        public int Memory { get; set; }
        public string Domain { get; set; }        
        public WindowsVersionDTO? WindowsVersion { get; set; }
        public VmmServerDTO VmmServer { get; set; }
        public bool IsSQLServer { get; set; }
        public bool IsInfraServer { get; set; }
        public string? Description { get; set; }
        public string Requester { get; set; }
        public VMMNetworkDTO? NetworkDTO { get; set; }
        public string? IPAddress { get; set; }
        public string? ServerName { get; set; }
        public DateTime CreationTime {get;set;}
        public bool ServerAutoStart { get; set; }
        public int? BlimpId { get; set; }
        public string? BlimpEnv { get; set; }
        public string? BlimpName { get; set; }

        public string? LeonRequestId { get; set; }
        public int Status { get; set; }

        public bool ManualOverride { get; set; } = false;

        public int Disk_D { get; set; } = 0;
        public int Disk_E { get; set; } = 0;
    }
}
