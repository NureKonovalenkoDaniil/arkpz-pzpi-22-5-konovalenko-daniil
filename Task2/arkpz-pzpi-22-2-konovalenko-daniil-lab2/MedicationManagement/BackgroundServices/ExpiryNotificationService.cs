using MedicationManagement.Services;

namespace MedicationManagement.BackgroundServices
{
    public class ExpiryNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ExpiryNotificationService(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var medicineService = scope.ServiceProvider.GetRequiredService<IServiceMedicine>();

                    var expiringMedicines = await medicineService.GetExpiringMedicines(DateTime.Now.AddDays(7));

                    if (expiringMedicines.Any())
                    {
                        foreach (var medicine in expiringMedicines)
                        {
                            Console.WriteLine($"Notify: Medicine {medicine.Name} is expiring on {medicine.ExpiryDate}.");
                            // Notify the user
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
