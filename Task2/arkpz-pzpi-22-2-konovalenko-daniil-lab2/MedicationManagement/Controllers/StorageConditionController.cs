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
    public class StorageConditionController : ControllerBase
    {
        private readonly IServiceStorageCondition _storageCondition;
        private readonly IServiceAuditLog _auditLogService;
        private readonly ILogger<StorageConditionController> _logger;

        public StorageConditionController(IServiceStorageCondition storageCondition, IServiceAuditLog auditLogService, ILogger<StorageConditionController> logger)
        {
            _storageCondition = storageCondition;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        [HttpGet("checkCondition")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CheckStorageConditionsForAllDevices()
        {
            var result = await _storageCondition.CheckStorageConditionsForAllDevices();
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] StorageCondition storageCondition)
        {
            if (ModelState.IsValid)
            {
                var result = await _storageCondition.Create(storageCondition);
                if (result != null)
                {
                    await _auditLogService.LogAction($"Create Condition.", User.Identity.Name, $"Created Condition: ${result.ConditionID}.");
                    return Ok(result);
                }
                return BadRequest("Condition is null");
            }
            return BadRequest(ModelState);
        }
        [HttpGet]
        public async Task<IActionResult> Read()
        {
            var result = await _storageCondition.Read();
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> ReadById(int id)
        {
            var result = await _storageCondition.ReadById(id);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound($"Condition with id: {id} not found");
        }
        [HttpPatch("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update([Bind("ConditionID,Temperature,Humidity,Timestamp,DeviceID,IoTDevice")] int id, StorageCondition storageCondition)
        {
            if (ModelState.IsValid)
            {
                var result = await _storageCondition.Update(id, storageCondition);
                if (result != null)
                {
                    await _auditLogService.LogAction($"Update Condition.", User.Identity.Name, $"Updated Condition: ${result.ConditionID}.");
                    return Ok(result);
                }
                return NotFound($"Condition with id: {id} not found");
            }
            return BadRequest(ModelState);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _storageCondition.Delete(id);
            if (result != null)
            {
                await _auditLogService.LogAction($"Delete Condition.", User.Identity.Name, $"Delete Condition: ${id}.");
                return Ok(result);
            }
            return NotFound($"Condition with id: {id} not found");
        }
    }
}
