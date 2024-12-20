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
    public class MedicineController : ControllerBase
    {
        private readonly IServiceMedicine _medicineService;
        private readonly IServiceAuditLog _auditLogService;
        private readonly ILogger<MedicineController> _logger;

        // Constructor to inject dependencies
        public MedicineController(IServiceMedicine medicineService, IServiceAuditLog auditLogService, ILogger<MedicineController> logger)
        {
            _medicineService = medicineService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        // Endpoint to get medicines with low stock
        [HttpGet("low-stock")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetLowStockMedicines(int threshold)
        {
            var medicines = await _medicineService.GetLowStockMedicines(threshold);
            return Ok(medicines);
        }

        // Endpoint to get medicines that are expiring before a certain date
        [HttpGet("expiring")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetExpiringMedicines()
        {
            var result = await _medicineService.GetExpiringMedicines(DateTime.Now);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }

        // Endpoint to get replenishment recommendations for low stock medicines
        [HttpGet("replenishment-recommendations")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetReplenishmentRecommendations()
        {
            var recommendations = await _medicineService.GetReplenishmentRecommendations();
            return Ok(recommendations);
        }

        // Endpoint to create a new medicine
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                var result = await _medicineService.Create(medicine);
                if (result != null)
                {
                    await _auditLogService.LogAction("Create Medicine", User.Identity.Name, $"Created medicine: {result.Name}.", false);
                    return Ok(result);
                }
                return BadRequest("Medication is null");
            }
            return BadRequest(ModelState);
        }

        // Endpoint to read all medicines
        [HttpGet]
        public async Task<IActionResult> Read()
        {
            var result = await _medicineService.Read();
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }

        // Endpoint to read a medicine by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> ReadById(int id)
        {
            var result = await _medicineService.ReadById(id);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound($"Medication with id: {id} not found");
        }

        // Endpoint to update an existing medicine
        [HttpPatch("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(int id, [FromBody] JsonPatchDocument<Medicine> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Patch document is null");
            }

            var result = await _medicineService.Update(id, patchDoc);
            if (result != null)
            {
                return Ok(result);
            }

            return NotFound($"Medication with id: {id} not found");
        }


        // Endpoint to delete a medicine by ID
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _medicineService.Delete(id);
            if (result)
            {
                await _auditLogService.LogAction("Delete Medicine", User.Identity.Name, $"Deleted Medicine: {id}.", false);
                return Ok($"Medication with id: {id} deleted");
            }
            return NotFound($"Medication with id: {id} not found");
        }
    }
}
