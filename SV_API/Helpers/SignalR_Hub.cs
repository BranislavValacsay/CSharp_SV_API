using Microsoft.AspNetCore.SignalR;

namespace sp_api.Helpers
{
    public class SignalR_Hub:Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("LogUpdate",message);
        }
    }
}
