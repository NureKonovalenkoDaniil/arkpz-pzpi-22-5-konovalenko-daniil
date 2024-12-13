using MedicationManagement.DBContext;
using MedicationManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicationManagement.Services
{
    public interface IServiceStorageCondition
    {
        public Task<List<string>> CheckStorageConditionsForAllDevices();
        public Task<int> GetTemperatureViolations(float minTemperature, float maxTemperature);
        public Task<int> GetHumidityViolations(float minHumidity, float maxHumidity);
        public Task<StorageCondition> Create(StorageCondition storageCondition);
        public Task<IEnumerable<StorageCondition>> Read();
        public Task<StorageCondition> ReadById(int id);
        public Task<StorageCondition> Update(int id, StorageCondition storageCondition);
        public Task<bool> Delete(int id);
    }
    public class ServiceStorageCondition : IServiceStorageCondition
    {
        private readonly MedicineStorageContext _context;
        private readonly ILogger<StorageCondition> _logger;

        public ServiceStorageCondition(MedicineStorageContext context, ILogger<StorageCondition> logger)
        {
            _context = context;
            _logger = logger;
        }
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


        public async Task<int> GetTemperatureViolations(float minTemperature, float maxTemperature)
        {
            return await _context.StorageConditions
                .Where(sc => sc.Temperature < minTemperature || sc.Temperature > maxTemperature)
                .CountAsync();
        }

        public async Task<int> GetHumidityViolations(float minHumidity, float maxHumidity)
        {
            return await _context.StorageConditions
                .Where(sc => sc.Humidity < minHumidity || sc.Humidity > maxHumidity)
                .CountAsync();
        }


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
            //if (storageCondition == null)
            //{
            //    _logger.LogError("Storage condition object is null");
            //    return null;
            //}
            //try
            //{
            //    await _context.StorageConditions.AddAsync(storageCondition);
            //    await _context.SaveChangesAsync();
            //    return storageCondition;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.Message);
            //    return null;
            //}
        }

        public async Task<IEnumerable<StorageCondition>> Read()
        {
            return await _context.StorageConditions
                .Include(sc => sc.IoTDevice)
                .ToListAsync();
        }

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

        public async Task<StorageCondition> Update(int id, StorageCondition storageCondition)
        {
            try
            {
                var storageConditionToUpdate = await _context.StorageConditions.FindAsync(id);
                if (storageConditionToUpdate == null)
                {
                    _logger.LogError("Condition not found");
                    return null;
                }
                _context.StorageConditions.Update(storageConditionToUpdate);
                await _context.SaveChangesAsync();
                return storageConditionToUpdate;
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
