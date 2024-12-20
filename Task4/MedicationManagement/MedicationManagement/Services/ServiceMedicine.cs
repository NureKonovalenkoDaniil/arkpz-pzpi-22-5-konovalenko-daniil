using Azure;
using MedicationManagement.DBContext;
using MedicationManagement.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace MedicationManagement.Services
{
    // Interface for the medicine service
    public interface IServiceMedicine
    {
        Task<List<ReplenishmentRecommendation>> GetReplenishmentRecommendations();
        Task<IEnumerable<Medicine>> GetExpiringMedicines(DateTime thresholdDate);
        Task<List<Medicine>> GetLowStockMedicines(int threshold);
        Task<Medicine> Create(Medicine medicine);
        Task<IEnumerable<Medicine>> Read();
        Task<Medicine> ReadById(int id);
        Task<Medicine> Update(int id, JsonPatchDocument<Medicine> patchDocument);
        Task<bool> Delete(int id);
    }
    // Implementation of the medicine service
    public class ServiceMedicine : IServiceMedicine
    {
        private readonly MedicineStorageContext _context;
        private readonly ILogger<ServiceMedicine> _logger;

        // Constructor to inject the database context and logger
        public ServiceMedicine(MedicineStorageContext context, ILogger<ServiceMedicine> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Method to get medicines with low stock
        public async Task<List<Medicine>> GetLowStockMedicines(int threshold)
        {
            return await _context.Medicines
                .Where(m => m.Quantity < threshold)
                .ToListAsync();
        }

        // Method to get medicines that are expiring before a certain date
        public async Task<IEnumerable<Medicine>> GetExpiringMedicines(DateTime thresholdDate)
        {
            thresholdDate = DateTime.Now;
            return await _context.Medicines
                .Where(m => m.ExpiryDate <= thresholdDate)
                .ToListAsync();
        }

        // Method to get replenishment recommendations for low stock medicines
        public async Task<List<ReplenishmentRecommendation>> GetReplenishmentRecommendations()
        {
            var lowStockMedicines = await GetLowStockMedicines(10);
            return lowStockMedicines.Select(m => new ReplenishmentRecommendation
            {
                MedicineId = m.MedicineID,
                MedicineName = m.Name,
                RecommendedQuantity = 100 - m.Quantity
            }).ToList();
        }

        // Method to create a new medicine
        public async Task<Medicine> Create(Medicine medicine)
        {
            if (medicine == null)
            {
                _logger.LogError("Medicine object is null");
                return null;
            }

            try
            {
                await _context.Medicines.AddAsync(medicine);
                await _context.SaveChangesAsync();
                return medicine;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        // Method to read all medicines
        public async Task<IEnumerable<Medicine>> Read()
        {
            return await _context.Medicines.ToListAsync();
        }

        // Method to read a medicine by ID
        public async Task<Medicine> ReadById(int id)
        {
            try
            {
                var medicine = await _context.Medicines.FindAsync(id);
                if (medicine == null)
                {
                    _logger.LogError("Medicine not found");
                    return null;
                }
                return medicine;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        // Method to update an existing medicine
        public async Task<Medicine> Update(int id, JsonPatchDocument<Medicine> patchDocument)
        {
            try
            {
                var medicineToUpdate = await _context.Medicines.FindAsync(id);
                if (medicineToUpdate == null)
                {
                    _logger.LogError($"Medicine with id: {id} not found");
                    return null;
                }

                patchDocument.ApplyTo(medicineToUpdate);

                await _context.SaveChangesAsync();
                return medicineToUpdate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the medicine");
                return null;
            }
        }

        // Method to delete a medicine by ID
        public async Task<bool> Delete(int id)
        {
            try
            {
                var medicine = await _context.Medicines.FindAsync(id);
                if (medicine == null)
                {
                    _logger.LogError("Medicine not found");
                    return false;
                }

                _context.Medicines.Remove(medicine);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}
