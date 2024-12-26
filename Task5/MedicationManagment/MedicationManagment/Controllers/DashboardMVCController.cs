using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicationManagement.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Доступ лише для авторизованих користувачів
    public class DashboardMVCController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
