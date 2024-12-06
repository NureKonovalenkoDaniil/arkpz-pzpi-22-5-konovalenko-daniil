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
        private readonly ILogger<IoTDeviceController> _logger;

        public IoTDeviceController(IServiceIoTDevice IoTDevice, ILogger<IoTDeviceController> logger)
        {
            _iotDeviceService = IoTDevice;
            _logger = logger;
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
                return Ok(result);
            }
            return NotFound($"IoT Device with id: {id} not found");
        }
    }
}
