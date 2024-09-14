namespace sp_api.Models
{
    public class StorageQoSPolicies
    {
        public string? name {  get; set; } //name and main DB key (string/varchar)
        public string? classification { get; set; } = "test"; //test or production
        public bool isSql { get; set; } = false; //is server is SQL server, then polici is SQL
        public bool allowed { get; set; } = false; //if policy is allowed for automation
    }
}
