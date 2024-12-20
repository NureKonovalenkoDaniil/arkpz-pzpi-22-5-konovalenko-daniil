using MedicationManagement.Services;

namespace MedicationManagement.BackgroundServices
{
    public class StorageConditionMonitoringService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public StorageConditionMonitoringService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var storageConditionService = scope.ServiceProvider.GetRequiredService<IServiceStorageCondition>();

                    var violations = await storageConditionService.CheckStorageConditionsForAllDevices();

                    foreach (var violation in violations)
                    {
                        Console.WriteLine($"Notification: {violation}");
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
