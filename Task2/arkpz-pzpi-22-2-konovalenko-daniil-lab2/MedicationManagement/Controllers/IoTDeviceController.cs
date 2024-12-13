using MedicationManagement.Models;
using MedicationManagement.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MedicationManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class IoTDeviceController : ControllerBase
    {
        private readonly IServiceIoTDevice _iotDeviceService;
        private readonly IServiceAuditLog _auditLogService;
        private readonly ILogger<IoTDeviceController> _logger;

        public IoTDeviceController(IServiceIoTDevice IoTDevice,IServiceAuditLog auditLogService ,ILogger<IoTDeviceController> logger)
        {
            _iotDeviceService = IoTDevice;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        [HttpPost("activate-sensor")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ActivateSensor(int sensorId)
        {
            var result = await _iotDeviceService.SetSensorStatus(sensorId, true);
            if (!result)
                return NotFound("Sensor not found");

            await _auditLogService.LogAction($"Activate Sensor.", User.Identity.Name, $"Activated sensor: ${sensorId}.");
            return Ok($"Sensor {sensorId} activated.");
        }

        [HttpPost("deactivate-sensor")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeactivateSensor(int sensorId)
        {
            var result = await _iotDeviceService.SetSensorStatus(sensorId, false);
            if (!result)
                return NotFound("Sensor not found");

            await _auditLogService.LogAction($"Deactivate Sensor.", User.Identity.Name, $"Deactivated sensor: ${sensorId}.");
            return Ok($"Sensor {sensorId} deactivated.");
        }

        [HttpGet("conditions/{deviceId}")]
        public async Task<IActionResult> GetConditionsByDeviceId(int deviceId)
        {
            var conditions = await _iotDeviceService.GetConditionsByDeviceId(deviceId);
            return Ok(conditions);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] IoTDevice IoTDevice)
        {
            if (ModelState.IsValid)
            {
                var result = await _iotDeviceService.Create(IoTDevice);
                if (result != null)
                {
                    await _auditLogService.LogAction($"Create Sensor.", User.Identity.Name, $"Created sensor: ${result.DeviceID}.");
                    return Ok(result);
                }
                return BadRequest("IoT device is null");
            }
            return BadRequest(ModelState);
        }
        [HttpGet]
        public async Task<IActionResult> Read()
        {
            var result = await _iotDeviceService.Read();
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> ReadById(int id)
        {
            var result = await _iotDeviceService.ReadById(id);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound($"IoT Device with id: {id} not found");
        }
        [HttpPatch("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update([Bind("DeviceID,Location,Type,Parameters")] int id, IoTDevice IoTDevice)
        {
            if (ModelState.IsValid)
            {
                var result = await _iotDeviceService.Update(id, IoTDevice);
                if (result != null)
                {
                    await _auditLogService.LogAction($"Update Sensor.", User.Identity.Name, $"Updated sensor: ${result.DeviceID}.");
                    return Ok(result);
                }
                return NotFound($"IoT Device with id: {id} not found");
            }
            return BadRequest(ModelState);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _iotDeviceService.Delete(id);
            if (result != null)
            {
                await _auditLogService.LogAction($"Delete Sensor.", User.Identity.Name, $"Deleted sensor: ${id}.");
                return Ok(result);
            }
            return NotFound($"IoT Device with id: {id} not found");
        }
    }
}
