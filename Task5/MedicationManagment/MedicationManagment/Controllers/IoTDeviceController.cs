using MedicationManagement.Models;
using MedicationManagement.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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

        // Constructor to inject dependencies
        public IoTDeviceController(IServiceIoTDevice iotDeviceService, IServiceAuditLog auditLogService, ILogger<IoTDeviceController> logger)
        {
            _iotDeviceService = iotDeviceService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        // Endpoint to activate a sensor
        [HttpPost("activate-sensor")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ActivateSensor(int sensorId)
        {
            var result = await _iotDeviceService.SetSensorStatus(sensorId, true);
            if (!result)
                return NotFound("Sensor not found");

            await _auditLogService.LogAction("Activate Sensor", User.Identity.Name, $"Activated sensor: {sensorId}.", false);
            return Ok($"Sensor {sensorId} activated.");
        }

        // Endpoint to deactivate a sensor
        [HttpPost("deactivate-sensor")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeactivateSensor(int sensorId)
        {
            var result = await _iotDeviceService.SetSensorStatus(sensorId, false);
            if (!result)
                return NotFound("Sensor not found");

            await _auditLogService.LogAction("Deactivate Sensor", User.Identity.Name, $"Deactivated sensor: {sensorId}.", false);
            return Ok($"Sensor {sensorId} deactivated.");
        }

        // Endpoint to get conditions by device ID
        [HttpGet("conditions/{deviceId}")]
        public async Task<IActionResult> GetConditionsByDeviceId(int deviceId)
        {
            var conditions = await _iotDeviceService.GetConditionsByDeviceId(deviceId);
            return Ok(conditions);
        }

        // Endpoint to create a new IoT device
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] IoTDevice iotDevice)
        {
            if (ModelState.IsValid)
            {
                var result = await _iotDeviceService.Create(iotDevice);
                if (result != null)
                {
                    await _auditLogService.LogAction("Create Sensor", User.Identity.Name, $"Created sensor: {result.DeviceID}.", false);
                    return Ok(result);
                }
                return BadRequest("IoT device is null");
            }
            return BadRequest(ModelState);
        }

        // Endpoint to read all IoT devices
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

        // Endpoint to read an IoT device by ID
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

        // Endpoint to update an existing IoT device
        [HttpPatch("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(int id, [FromBody] JsonPatchDocument<IoTDevice> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Patch document is null");
            }

            var result = await _iotDeviceService.Update(id, patchDoc);
            if (result != null)
            {
                return Ok(result);
            }

            return NotFound($"Device with id: {id} not found");
        }

        // Endpoint to delete an IoT device by ID
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _iotDeviceService.Delete(id);
            if (result)
            {
                await _auditLogService.LogAction("Delete Sensor", User.Identity.Name, $"Deleted sensor: {id}.", false);
                return Ok($"IoT Device with id: {id} deleted");
            }
            return NotFound($"IoT Device with id: {id} not found");
        }
    }
}
