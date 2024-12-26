using MedicationManagement.DBContext;
using MedicationManagement.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicationManagement.Services
{
    // Interface for the IoT device service
    public interface IServiceIoTDevice
    {
        Task<bool> SetSensorStatus(int sensorId, bool isActive);
        Task<List<StorageCondition>> GetConditionsByDeviceId(int deviceId);
        Task<IoTDevice> Create(IoTDevice IoTDevice);
        Task<IEnumerable<IoTDevice>> Read();
        Task<IoTDevice> ReadById(int id);
        Task<IoTDevice> Update(int id, JsonPatchDocument<IoTDevice> patchDocument);
        Task<bool> Delete(int id);
    }
    // Implementation of the IoT device service
    public class ServiceIoTDevice : IServiceIoTDevice
    {
        private readonly MedicineStorageContext _context;
        private readonly ILogger<ServiceIoTDevice> _logger;

        // Constructor to inject the database context and logger
        public ServiceIoTDevice(MedicineStorageContext context, ILogger<ServiceIoTDevice> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Method to set the status of a sensor
        public async Task<bool> SetSensorStatus(int sensorId, bool isActive)
        {
            var sensor = await _context.IoTDevices.FindAsync(sensorId);
            if (sensor == null)
            {
                _logger.LogError("Sensor not found");
                return false;
            }

            sensor.IsActive = isActive;
            _context.IoTDevices.Update(sensor);
            await _context.SaveChangesAsync();
            return true;
        }

        // Method to get storage conditions by device ID
        public async Task<List<StorageCondition>> GetConditionsByDeviceId(int deviceId)
        {
            return await _context.StorageConditions
                .Where(sc => sc.DeviceID == deviceId)
                .Include(sc => sc.IoTDevice)
                .ToListAsync();
        }

        // Method to create a new IoT device
        public async Task<IoTDevice> Create(IoTDevice IoTDevice)
        {
            if (IoTDevice == null)
            {
                _logger.LogError("IoT device object is null");
                return null;
            }

            try
            {
                await _context.IoTDevices.AddAsync(IoTDevice);
                await _context.SaveChangesAsync();
                return IoTDevice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        // Method to read all IoT devices
        public async Task<IEnumerable<IoTDevice>> Read()
        {
            return await _context.IoTDevices.ToListAsync();
        }

        // Method to read an IoT device by ID
        public async Task<IoTDevice> ReadById(int id)
        {
            try
            {
                var IoTDevice = await _context.IoTDevices.FindAsync(id);
                if (IoTDevice == null)
                {
                    _logger.LogError("IoT device not found");
                    return null;
                }
                return IoTDevice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        // Method to update an existing IoT device
        public async Task<IoTDevice> Update(int id, JsonPatchDocument<IoTDevice> patchDocument)
        {
            try
            {
                var deviceToUpdate = await _context.IoTDevices.FindAsync(id);
                if (deviceToUpdate == null)
                {
                    _logger.LogError($"Device with id: {id} not found");
                    return null;
                }

                patchDocument.ApplyTo(deviceToUpdate);

                await _context.SaveChangesAsync();
                return deviceToUpdate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the device");
                return null;
            }
        }

        // Method to delete an IoT device by ID
        public async Task<bool> Delete(int id)
        {
            try
            {
                var IoTDevice = await _context.IoTDevices.FindAsync(id);
                if (IoTDevice == null)
                {
                    _logger.LogError("IoT device not found");
                    return false;
                }

                _context.IoTDevices.Remove(IoTDevice);
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
