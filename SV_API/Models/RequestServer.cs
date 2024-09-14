﻿﻿using System.Diagnostics.Eventing.Reader;
using System.Security.Policy;

namespace sp_api.Models
{
    public class RequestServer
    {
        public required string Guid { get; set; }
        public int CPU { get; set; }
        public int Memory { get; set; }
        public string? Domain { get; set; }
        
        public int WindowsVersionId { get; set; }
        public WindowsVersion? WindowsVersion { get; set; }
      
        public bool IsSQLServer { get; set; }
        public bool IsInfraServer { get; set; }
        public string? Description { get; set; }
        public string? Requester { get; set; }

        public string? NetworkId { get; set; }
        public VMMNetwork? VMMNetwork { get; set; }

        public string? IPAddress { get; set; }
        public string? ServerName { get; set; }
        public DateTime CreationTime { get; set; }

        public int? VMMServerId { get; set; }
        public VMMServer? VMMServer { get; set; }

        public int? BlimpId { get; set; }
        public string? BlimpEnv { get; set; }
        public string? BlimpName { get; set; }

        public string? LeonRequestId { get; set; }
        public int Status { get; set; }
        public bool ServerAutoStart { get; set; } = true;
        public bool ManualOverride { get; set; } = false;

        public int Disk_D { get; set; }
        public int Disk_E { get; set; }
    } 
}