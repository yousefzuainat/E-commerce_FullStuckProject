using Microsoft.AspNetCore.Mvc;

namespace ecommerce_system.Areas.Admin
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
