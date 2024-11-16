using Back.Controllers;
using Back.Utils;
using front.Utils.Logger;

namespace Back.Services
{
    public class ScheduledRoleUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _scheduledTime;

        public ScheduledRoleUpdateService(IServiceProvider serviceProvider, TimeSpan scheduledTime)
        {
            _serviceProvider = serviceProvider;
            _scheduledTime = scheduledTime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRunTime = DateTime.Today.Add(_scheduledTime);
                if (now > nextRunTime)
                {
                    nextRunTime = nextRunTime.AddDays(1);
                }

                var delay = nextRunTime - now;
                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userUpdateProxy = scope.ServiceProvider.GetRequiredService<UserUpdateProxy>();
                        await userUpdateProxy.ApplyPendingRoleUpdatesAsync();
                    }

                    try
                    {
                        LogsController.GetListOfLogFiles();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
    }
}
