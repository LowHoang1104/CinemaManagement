using Microsoft.AspNetCore.Mvc;

namespace CinemaManagement.Controllers
{
    public class TicketController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
