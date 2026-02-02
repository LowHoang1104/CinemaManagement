using Microsoft.AspNetCore.Mvc;

namespace CinemaManagement.Controllers
{
    public class MovieController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
