using MedicationManagement.DBContext;
using MedicationManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicationManagement.Services
{
    public interface IServiceIoTDevice
    {
        public Task<bool> SetSensorStatus(int sensorId, bool isActive);
        public Task<List<StorageCondition>> GetConditionsByDeviceId(int deviceId);
        public Task<IoTDevice> Create(IoTDevice IoTDevice);
        public Task<IEnumerable<IoTDevice>> Read();
        public Task<IoTDevice> ReadById(int id);
        public Task<IoTDevice> Update(int id, IoTDevice IoTDevice);
        public Task<bool> Delete(int id);
    }
    public class ServiceIoTDevice : IServiceIoTDevice
    {
        private readonly MedicineStorageContext _context;
        private readonly ILogger<ServiceIoTDevice> _logger;

        public ServiceIoTDevice(MedicineStorageContext context, ILogger<ServiceIoTDevice> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> SetSensorStatus(int sensorId, bool isActive)
        {
            var sensor = await _context.IoTDevices.FindAsync(sensorId);
            if (sensor == null)
                return false;

            sensor.IsActive = isActive;
            _context.IoTDevices.Update(sensor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<StorageCondition>> GetConditionsByDeviceId(int deviceId)
        {
            return await _context.StorageConditions
                .Where(sc => sc.DeviceID == deviceId)
                .Include(sc => sc.IoTDevice)
                .ToListAsync();
        }

        public async Task<IoTDevice> Create(IoTDevice IoTDevices)
        {
            if (IoTDevices == null)
            {
                _logger.LogError("IoT device object is null");
                return null;
            }
            try
            {
                await _context.IoTDevices.AddAsync(IoTDevices);
                await _context.SaveChangesAsync();
                return IoTDevices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<IoTDevice>> Read()
        {
            return await _context.IoTDevices.ToListAsync();
        }

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

        public async Task<IoTDevice> Update(int id, IoTDevice IoTDevices)
        {
            try
            {
                var IoTDeviceToUpdate = await _context.IoTDevices.FindAsync(id);
                if (IoTDeviceToUpdate == null)
                {
                    _logger.LogError("IoT device not found");
                    return null;
                }
                _context.IoTDevices.Update(IoTDeviceToUpdate);
                await _context.SaveChangesAsync();
                return IoTDeviceToUpdate;
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
                    _logger.LogError("IoT device not found");
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
