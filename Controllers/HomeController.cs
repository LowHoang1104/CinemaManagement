using System.Diagnostics;
using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, CinemaManagementContext context)
     : base(context)
 {
      _logger = logger;
   }

   public IActionResult Index()
        {
        var selectedCinemaId = GetSelectedCinemaId();
      ViewData["SelectedCinemaId"] = selectedCinemaId;

      // Get movies with showtimes for selected cinema
       var nowUtc = DateTime.UtcNow;
        var moviesWithShowTimes = new List<Movie>();

  if (selectedCinemaId != Guid.Empty)
{
        moviesWithShowTimes = _context.Movies
      .Where(m => m.Status == 1)
         .Include(m => m.ShowTimes)
   .ThenInclude(st => st.Room)
   .ThenInclude(r => r.Cinema)
         .Where(m => m.ShowTimes.Any(st => 
  st.Room.Cinema.CinemaId == selectedCinemaId && 
   st.StartAt > nowUtc && 
       st.Status == 1))
        .OrderBy(x => x.CreatedAt)
    .ToList();
  }

            // Initialize user session with first user from database
         InitializeUserSession();

   return View(moviesWithShowTimes);
    }

     private void InitializeUserSession()
        {
         var userIdStr = HttpContext.Session.GetString("UserId");
 if (userIdStr == null)
   {
       // Get first user from database
  var firstUser = _context.Users.FirstOrDefault();

       if (firstUser != null)
  {
  // Save user info to session
HttpContext.Session.SetString("UserId", firstUser.UserId.ToString());
     HttpContext.Session.SetString("UserEmail", firstUser.Email ?? "");
    HttpContext.Session.SetString("UserFullName", firstUser.FullName ?? "");
       }
      }
     }

        [HttpPost]
   public IActionResult SelectCinema([FromBody] SelectCinemaRequest request)
 {
            try
   {
  var cinema = _context.Cinemas.FirstOrDefault(x => x.CinemaId == request.CinemaId && x.Status == 1);

            if (cinema == null)
       {
       return Json(new { success = false, message = "C? s? không h?p l?" });
    }

  // Save to session
      HttpContext.Session.SetString("SelectedCinemaId", request.CinemaId.ToString());
      HttpContext.Session.SetString("SelectedCinemaName", cinema.Name);

       return Json(new { success = true, cinemaName = cinema.Name });
    }
            catch (Exception ex)
      {
      _logger.LogError(ex, "Error selecting cinema");
       return Json(new { success = false, message = "Có l?i x?y ra khi ch?n c? s?" });
   }
   }

   [HttpGet]
        public IActionResult GetSelectedCinema()
    {
   var cinemaId = HttpContext.Session.GetString("SelectedCinemaId");
            var cinemaName = HttpContext.Session.GetString("SelectedCinemaName");

        if (string.IsNullOrEmpty(cinemaId))
         {
 // Return first cinema as default
   var firstCinema = _context.Cinemas.Where(x => x.Status == 1).OrderBy(x => x.Name).FirstOrDefault();
                if (firstCinema != null)
 {
    // Set as default in session
       HttpContext.Session.SetString("SelectedCinemaId", firstCinema.CinemaId.ToString());
      HttpContext.Session.SetString("SelectedCinemaName", firstCinema.Name);
      return Json(new { cinemaId = firstCinema.CinemaId, cinemaName = firstCinema.Name });
     }
 return Json(new { cinemaId = "", cinemaName = "Ch?a ch?n c? s?" });
            }

      return Json(new { cinemaId = cinemaId, cinemaName = cinemaName });
      }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
{
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
     }
    }

 public class SelectCinemaRequest
 {
     public Guid CinemaId { get; set; }
    }
}
