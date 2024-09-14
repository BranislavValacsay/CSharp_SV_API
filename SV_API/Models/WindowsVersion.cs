namespace sp_api.Models
{
    public class WindowsVersion
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Version { get; set; }
        public string? ImageName {  get; set; }
        public ICollection<RequestServer> RequestServer { get; set; }
    }
}
