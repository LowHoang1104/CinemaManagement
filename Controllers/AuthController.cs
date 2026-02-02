using Microsoft.AspNetCore.Mvc;

namespace CinemaManagement.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Otp()
        {
            return View();
        }
        public IActionResult ResetPassword()
        {
            return View();
        }
        public IActionResult EmailVerify()
        {
            return View();
        }
    }
}
