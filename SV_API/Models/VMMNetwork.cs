namespace sp_api.Models
{
    public class VMMNetwork
    {
        public required string Name { get; set; }
        public string? LogicalNetworkDefinition { get; set; }
        public int VlanID { get; set; } 
        public string? Subnet { get; set; }
        public int Cidr { get; set; }
        public string? Gateway { get; set; }
        public bool isActive { get; set; }
        public ICollection<RequestServer> RequestServer { get; set; }
    }
}
