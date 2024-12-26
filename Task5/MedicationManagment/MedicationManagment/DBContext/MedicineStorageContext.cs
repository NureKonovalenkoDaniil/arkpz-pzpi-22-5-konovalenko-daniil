using MedicationManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedicationManagement.DBContext
{
    public class MedicineStorageContext : DbContext
    {
        public DbSet<StorageCondition> StorageConditions { get; set; }
        public DbSet<IoTDevice> IoTDevices { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public MedicineStorageContext(DbContextOptions<MedicineStorageContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StorageCondition>()
                .Property(sc => sc.Timestamp)
                .HasColumnType("timestamp without time zone"); // PostgreSQL type

            modelBuilder.Entity<Medicine>()
                .Property(m => m.ExpiryDate)
                .HasColumnType("timestamp without time zone"); // PostgreSQL type
        }

    }
}
