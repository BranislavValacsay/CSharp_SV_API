namespace sp_api.Models
{
    public class VMMLocation
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<VMMServer>? VMMServer { get; set; }
    }
}
