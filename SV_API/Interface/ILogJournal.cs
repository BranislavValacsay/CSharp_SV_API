using sp_api.Models;

namespace sp_api.Interface
{
    public interface ILogJournal
    {
        public Task SendLog(Log log);
        public Task<IEnumerable<Log>> GetLog();
        public Task<IEnumerable<Log>> GetLogPerGuid(string guid);

    }
}
