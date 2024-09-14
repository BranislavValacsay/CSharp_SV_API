namespace sp_api.DTO
{
    public class VMMNetworkDTO
    {
        public string Name { get; set; }
        public string LogicalNetworkDefinition { get; set; }
        public int VlanID { get; set; } 
        public string Subnet { get; set; }
        public int Cidr { get; set; }
        public string? Gateway { get; set; }
        public bool isActive { get; set; }
    }
}
