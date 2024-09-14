namespace sp_api.Models
{
    public class VMMServer
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public int LocationId {  get; set; }
        public VMMLocation Location { get; set; }

        public ICollection<RequestServer> RequestServer { get; set; }
    }
}
