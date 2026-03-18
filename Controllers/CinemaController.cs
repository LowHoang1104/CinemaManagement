using CinemaManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    public class CinemaController : BaseController
    {
        public CinemaController(CinemaManagementContext context) : base(context)
        {
        }

        public IActionResult Index()
        {
            var rooms = _context.Rooms
                .Include(r => r.Cinema)
                .Where(r => r.Status == 1 && r.Cinema.Status == 1)
                .OrderBy(r => r.Cinema.Name)
                .ThenBy(r => r.Name)
                .ToList();

            return View(rooms);
        }
    }
}
