using CinemaManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    public class MovieController : BaseController
    {
        public MovieController(CinemaManagementContext context) : base(context)
        {
        }

        public IActionResult Index()
        {
            var selectedCinemaId = GetSelectedCinemaId();
            var nowUtc = DateTime.UtcNow;

            var movies = _context.Movies
                .Include(m => m.ShowTimes.Where(st => st.Status == 1 && st.StartAt >= nowUtc))
                .ThenInclude(st => st.Room)
                .ThenInclude(r => r.Cinema)
                .Where(m => m.Status == 1)
                .Where(m => selectedCinemaId == Guid.Empty || m.ShowTimes.Any(st => st.Room.Cinema.CinemaId == selectedCinemaId))
                .OrderByDescending(m => m.ReleaseDate ?? DateTime.MinValue)
                .ThenBy(m => m.Title)
                .ToList();

            ViewData["SelectedCinemaId"] = selectedCinemaId;
            return View(movies);
        }

        public IActionResult Detail(Guid id)
        {
            var movie = _context.Movies.FirstOrDefault(x => x.MovieId == id);

            if (movie == null)
                return NotFound();

            var selectedCinemaId = GetSelectedCinemaId();

            // Convert to UTC for proper comparison with database
            var nowUtc = DateTime.UtcNow;

            // Get cinemas with their show times for this movie (filtered by selected cinema if any)
            var cinemas = _context.Cinemas
                .Include(c => c.Rooms)
                .ThenInclude(r => r.ShowTimes.Where(st => st.MovieId == id && st.StartAt > nowUtc && st.Status == 1))
                .Where(c => c.Status == 1 && (selectedCinemaId == Guid.Empty || c.CinemaId == selectedCinemaId))
                .OrderBy(c => c.Name)
                .ToList();

            // Group show times by room format
            var cinemaShowTimes = new List<dynamic>();

            foreach (var cinema in cinemas)
            {
                var roomShowTimes = new List<dynamic>();

                foreach (var room in cinema.Rooms.Where(r => r.Status == 1))
                {
                    foreach (var showTime in room.ShowTimes)
                    {
                        roomShowTimes.Add(new
                        {
                            RoomId = room.RoomId,
                            RoomName = room.Name,
                            ShowTimeId = showTime.ShowTimeId,
                            StartTime = showTime.StartAt,
                            BasePrice = showTime.BasePrice,
                            Format = showTime.Format ?? "2D PHỤ ĐỀ VIỆT"
                        });
                    }
                }

                if (roomShowTimes.Any())
                {
                    cinemaShowTimes.Add(new
                    {
                        CinemaId = cinema.CinemaId,
                        CinemaName = cinema.Name,
                        ShowTimes = roomShowTimes.GroupBy(x => x.Format).ToList()
                    });
                }
            }

            ViewData["CinemaShowTimes"] = cinemaShowTimes;
            ViewData["SelectedCinemaId"] = selectedCinemaId;

            return View(movie);
        }
    }
}
