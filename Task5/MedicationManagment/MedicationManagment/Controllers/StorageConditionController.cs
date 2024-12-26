using MedicationManagement.Models;
using MedicationManagement.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MedicationManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageConditionController : ControllerBase
    {
        private readonly IServiceStorageCondition _storageConditionService;
        private readonly IServiceAuditLog _auditLogService;
        private readonly ILogger<StorageConditionController> _logger;

        // Constructor to inject dependencies
        public StorageConditionController(IServiceStorageCondition storageConditionService, IServiceAuditLog auditLogService, ILogger<StorageConditionController> logger)
        {
            _storageConditionService = storageConditionService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        // Endpoint to check storage conditions for all devices
        [HttpGet("checkCondition")]
        public async Task<IActionResult> CheckStorageConditionsForAllDevices()
        {
            var result = await _storageConditionService.CheckStorageConditionsForAllDevices();
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }

        // Endpoint to create a new storage condition
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StorageCondition storageCondition)
        {
            if (ModelState.IsValid)
            {
                var result = await _storageConditionService.Create(storageCondition);
                if (result != null)
                {
                    string source = User.Identity?.Name ?? $"Sensor {storageCondition.DeviceID}";
                    bool isSensor = User.Identity == null;
                    await _auditLogService.LogAction("Create Condition", source, $"Created Condition: {result.ConditionID}.", isSensor);
                    return Ok(result);
                }
                return BadRequest("Condition is null");
            }
            return BadRequest(ModelState);
        }

        // Endpoint to read all storage conditions
        [HttpGet]
        public async Task<IActionResult> Read()
        {
            var result = await _storageConditionService.Read();
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }

        // Endpoint to read a storage condition by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> ReadById(int id)
        {
            var result = await _storageConditionService.ReadById(id);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound($"Condition with id: {id} not found");
        }

        // Endpoint to update an existing storage condition
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] JsonPatchDocument<StorageCondition> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Patch document is null");
            }

            var result = await _storageConditionService.Update(id, patchDoc);
            if (result != null)
            {
                return Ok(result);
            }

            return NotFound($"Condition with id: {id} not found");
        }

        // Endpoint to delete a storage condition by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _storageConditionService.Delete(id);
            if (result)
            {
                string source = User.Identity?.Name ?? $"Sensor {id}";
                bool isSensor = User.Identity == null;
                await _auditLogService.LogAction("Delete Condition", source, $"Deleted Condition: {id}.", isSensor);
                return Ok(result);
            }
            return NotFound($"Condition with id: {id} not found");
        }
    }
}
