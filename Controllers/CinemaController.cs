using Microsoft.AspNetCore.Mvc;

namespace CinemaManagement.Controllers
{
    public class CinemaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
