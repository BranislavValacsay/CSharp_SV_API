using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace sp_api.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string? Guid { get; set; }
        public MessageType MessageType { get; set; }
        public string? Command { get; set; }
        public string? Result {  get; set; }
        public string? MessageBody { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;

    }

    public enum MessageType
    {
        Command,
        Message,
        Warning,
        Error
    }
}
