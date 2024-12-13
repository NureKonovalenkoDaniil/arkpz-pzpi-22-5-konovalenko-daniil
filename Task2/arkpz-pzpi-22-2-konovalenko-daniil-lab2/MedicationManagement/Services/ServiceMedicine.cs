using MedicationManagement.DBContext;
using MedicationManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicationManagement.Services
{
    public interface IServiceMedicine
    {
        public Task<List<ReplenishmentRecommendation>> GetReplenishmentRecommendations();
        public Task<IEnumerable<Medicine>> GetExpiringMedicines(DateTime thresholdDate);
        public Task<List<Medicine>> GetLowStockMedicines(int threshold);
        public Task<Medicine> Create(Medicine medicine);
        public Task<IEnumerable<Medicine>> Read();
        public Task<Medicine> ReadById(int id);
        public Task<Medicine> Update(int id, Medicine medicine);
        public Task<bool> Delete(int id);
    }
    public class ServiceMedicine : IServiceMedicine
    {
        private readonly MedicineStorageContext _context;
        private readonly ILogger<ServiceMedicine> _logger;

        public ServiceMedicine(MedicineStorageContext context, ILogger<ServiceMedicine> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<Medicine>> GetLowStockMedicines(int threshold)
        {
            return await _context.Medicines
                .Where(m => m.Quantity < threshold)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medicine>> GetExpiringMedicines(DateTime thresholdDate)
        {
            var expiringMedicines = await _context.Medicines.Where(m => m.ExpiryDate <= thresholdDate).ToListAsync();
            return expiringMedicines;
        }
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

        public async Task<IEnumerable<Medicine>> Read()
        {
            return await _context.Medicines.ToListAsync();
        }

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

        public async Task<Medicine> Update(int id, Medicine medicine)
        {
            try
            {
                var medicineToUpdate = await _context.Medicines.FindAsync(id);
                if (medicineToUpdate == null)
                {
                    _logger.LogError("Medicine not found");
                    return null;
                }
                medicineToUpdate.Name = medicine.Name;
                medicineToUpdate.Type = medicine.Type;
                medicineToUpdate.ExpiryDate = medicine.ExpiryDate;
                medicineToUpdate.Quantity = medicine.Quantity;
                medicineToUpdate.Category = medicine.Category;
                _context.Medicines.Update(medicineToUpdate);
                await _context.SaveChangesAsync();
                return medicineToUpdate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

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
