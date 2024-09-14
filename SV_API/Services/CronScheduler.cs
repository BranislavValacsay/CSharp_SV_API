using sp_api.Data;
using sp_api.Interface;
using sp_api.Models;

namespace sp_api.Services
{
    public class CronScheduler : BackgroundService
    {
        private readonly TimeSpan _timeSpan = TimeSpan.FromSeconds(30);
        private readonly IServiceScopeFactory? _scopeFactory;

        public CronScheduler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                await RunOrchestrator();
                await Task.Delay(_timeSpan,cancellationToken);
 
            }
        }
        private async Task RunOrchestrator() // first GUID of request that is approved and not finished
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var orchestrator = scope.ServiceProvider.GetRequiredService<IOrchestrator>();
                var logJournal = scope.ServiceProvider.GetRequiredService<ILogJournal>();
                await orchestrator.MainOrchestrationLoop();
            }
        }
        
    }
}
