namespace sp_api.Models
{
    public class vServerProperties
    {
        public string? name {  get; set; }
        public string? virtualHardDisks { get; set; }
        public string? virtualMachineState { get; set; }
        public string? location { get; set; }
        public string? operatingSystem { get; set; }
        public string? vmhost { get; set; }
    }
}
