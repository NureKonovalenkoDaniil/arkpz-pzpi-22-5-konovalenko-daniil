using MedicationManagement.DBContext;
using MedicationManagement.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicationManagement.Services
{
    // Interface for the storage condition service
    public interface IServiceStorageCondition
    {
        Task<List<string>> CheckStorageConditionsForAllDevices();
        Task<StorageCondition> Create(StorageCondition storageCondition);
        Task<IEnumerable<StorageCondition>> Read();
        Task<StorageCondition> ReadById(int id);
        Task<StorageCondition> Update(int id, JsonPatchDocument<StorageCondition> patchDocument);
        Task<bool> Delete(int id);
    }
    // Implementation of the storage condition service
    public class ServiceStorageCondition : IServiceStorageCondition
    {
        private readonly MedicineStorageContext _context;
        private readonly ILogger<ServiceStorageCondition> _logger;

        // Constructor to inject the database context and logger
        public ServiceStorageCondition(MedicineStorageContext context, ILogger<ServiceStorageCondition> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Method to check storage conditions for all devices and log violations
        public async Task<List<string>> CheckStorageConditionsForAllDevices()
        {
            var devices = await _context.IoTDevices.ToListAsync();
            var violations = new List<string>();

            foreach (var device in devices)
            {
                var conditions = await _context.StorageConditions
                    .Where(sc => sc.DeviceID == device.DeviceID)
                    .OrderByDescending(sc => sc.Timestamp)
                    .FirstOrDefaultAsync();

                if (conditions != null)
                {
                    if (conditions.Temperature < device.MinTemperature || conditions.Temperature > device.MaxTemperature)
                    {
                        violations.Add($"Temperature violation for Device {device.DeviceID} at {conditions.Timestamp}: {conditions.Temperature}°C (Expected: {device.MinTemperature}–{device.MaxTemperature}°C)");
                    }

                    if (conditions.Humidity < device.MinHumidity || conditions.Humidity > device.MaxHumidity)
                    {
                        violations.Add($"Humidity violation for Device {device.DeviceID} at {conditions.Timestamp}: {conditions.Humidity}% (Expected: {device.MinHumidity}–{device.MaxHumidity}%)");
                    }
                }
            }

            return violations;
        }

        // Method to create a new storage condition
        public async Task<StorageCondition> Create(StorageCondition storageCondition)
        {
            var device = await _context.IoTDevices.FindAsync(storageCondition.DeviceID);
            if (device == null)
            {
                _logger.LogError($"IoTDevice with ID {storageCondition.DeviceID} does not exist.");
                return null;
            }

            storageCondition.Timestamp = DateTime.Now;
            await _context.StorageConditions.AddAsync(storageCondition);
            await _context.SaveChangesAsync();

            return storageCondition;
        }

        // Method to read all storage conditions
        public async Task<IEnumerable<StorageCondition>> Read()
        {
            return await _context.StorageConditions
                .Include(sc => sc.IoTDevice)
                .ToListAsync();
        }

        // Method to read a storage condition by ID
        public async Task<StorageCondition> ReadById(int id)
        {
            try
            {
                var storageCondition = await _context.StorageConditions
                    .Include(sc => sc.IoTDevice)
                    .FirstOrDefaultAsync(sc => sc.ConditionID == id);
                if (storageCondition == null)
                {
                    _logger.LogError("Condition not found");
                    return null;
                }
                return storageCondition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        // Method to update an existing storage condition
        public async Task<StorageCondition> Update(int id, JsonPatchDocument<StorageCondition> patchDocument)
        {
            try
            {
                var conditionToUpdate = await _context.StorageConditions.FindAsync(id);
                if (conditionToUpdate == null)
                {
                    _logger.LogError($"Condition with id: {id} not found");
                    return null;
                }

                patchDocument.ApplyTo(conditionToUpdate);

                await _context.SaveChangesAsync();
                return conditionToUpdate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the condition");
                return null;
            }
        }

        // Method to delete a storage condition by ID
        public async Task<bool> Delete(int id)
        {
            try
            {
                var storageCondition = await _context.StorageConditions.FindAsync(id);
                if (storageCondition == null)
                {
                    _logger.LogError("Condition not found");
                    return false;
                }

                _context.StorageConditions.Remove(storageCondition);
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
