using MedicationManagement.DBContext;
using MedicationManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicationManagement.Services
{
    public interface IServiceAuditLog
    {
        public Task LogAction(string action, string user, string details);
    }
    public class ServiceAuditLog : IServiceAuditLog
    {
        private readonly MedicineStorageContext _context;
        public ServiceAuditLog(MedicineStorageContext context)
        {
            _context = context;
        }
        public async Task LogAction(string action, string user, string details)
        {
            var log = new AuditLog
            {
                Action = action,
                User = user,
                Timestamp = DateTime.UtcNow,
                Details = details
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
