using Microsoft.AspNetCore.Mvc;

namespace MedicationManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
