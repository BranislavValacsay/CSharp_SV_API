namespace sp_api.DTO
{
    public class PUT_RequestServer_DTO
    {   public string? Guid { get; set; }
        public int CPU { get; set; }
        public int Memory { get; set; }
        public string Domain { get; set; }
        public int WindowsVersionId { get; set; }
        public int VMMServerId { get; set; }
        public bool IsSQLServer { get; set; }
        public bool IsInfraServer { get; set; }
        public string? Description { get; set; }
        public string Requester { get; set; }
        public string? NetworkId { get; set; }
        public string? IPAddress { get; set; }
        public string? ServerName { get; set; }
        public int? BlimpId { get; set; }
        public string? BlimpEnv { get; set; }
        public string? BlimpName { get; set; }
        public string? LeonRequestId { get; set; }
        public bool ManualOverride { get; set; } = false;
        public int Disk_D { get; set; }
        public int Disk_E { get; set; }
    }
}
