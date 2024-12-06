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
    public class MedicineController : ControllerBase
    {
        private readonly IServiceMedicine _medicineService;
        private readonly ILogger<MedicineController> _logger;

        public MedicineController(IServiceMedicine medicineService, ILogger<MedicineController> logger)
        {
            _medicineService = medicineService;
            _logger = logger;
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockMedicines(int threshold)
        {
            var medicines = await _medicineService.GetLowStockMedicines(threshold);
            return Ok(medicines);
        }

        [HttpPost("expiring")]
        public async Task<IActionResult> GetExpiringMedicines(DateTime thresholdDate)
        {
            var result = await _medicineService.GetExpiringMedicines(thresholdDate);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                var result = await _medicineService.Create(medicine);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest("Medication is null");
            }
            return BadRequest(ModelState);
        }
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
        [HttpPatch("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update([Bind("MedicineID,Name,Type,ExpiryDate,QuantityCategory")] int id, Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                var result = await _medicineService.Update(id, medicine);
                if (result != null)
                {
                    return Ok(result);
                }
                return NotFound($"Medication with id: {id} not found");
            }
            return BadRequest(ModelState);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _medicineService.Delete(id);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound($"Medication with id: {id} not found");
        }
    }
}
