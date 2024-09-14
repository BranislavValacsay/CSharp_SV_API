
using sp_api.Helpers;
using sp_api.Interface;
using sp_api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace sp_api.Data
{
    public class LogJournal:ILogJournal
    {
        private readonly API_DbContext _context;
        private readonly IHubContext<SignalR_Hub> _hub;

        public LogJournal(API_DbContext context, IHubContext<SignalR_Hub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task SendLog(Log log)
        {
            _context.LoggingSystem.Add(log);
            await _hub.Clients.All.SendAsync("LogUpdate", log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Log>> GetLog()
        {
            var result = await _context.LoggingSystem.ToListAsync();
            return result;
        }

        public async Task<IEnumerable<Log>> GetLogPerGuid(string guid)
        {
            var result = await _context.LoggingSystem
                .Where(x => x.Guid == guid)
                .ToListAsync();

            return result;
        }
    }
}
