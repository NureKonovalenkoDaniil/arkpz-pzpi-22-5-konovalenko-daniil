using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicationManagement.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DataMVCController : Controller
    {
        public IActionResult Medicines()
        {
            return View("Medicines");
        }

        public IActionResult IoTDevices()
        {
            return View();
        }

        public IActionResult StorageConditions()
        {
            return View();
        }
    }
}
