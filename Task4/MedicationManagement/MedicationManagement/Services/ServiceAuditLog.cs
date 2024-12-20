using MedicationManagement.DBContext;
using MedicationManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicationManagement.Services
{
    // Interface for the audit log service
    public interface IServiceAuditLog
    {
        Task LogAction(string action, string user, string details, bool isSensor);
    }
    // Implementation of the audit log service
    public class ServiceAuditLog : IServiceAuditLog
    {
        private readonly MedicineStorageContext _context;

        // Constructor to inject the database context
        public ServiceAuditLog(MedicineStorageContext context)
        {
            _context = context;
        }

        // Method to log an action to the audit log
        public async Task LogAction(string action, string userOrDevice, string details, bool isSensor = false)
        {
            var log = new AuditLog
            {
                Action = action,
                User = isSensor ? $"Sensor {userOrDevice}" : userOrDevice,
                Timestamp = DateTime.UtcNow,
                Details = details
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

    }
}
