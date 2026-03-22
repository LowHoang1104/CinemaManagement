using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    public class TicketController : BaseController
 {
 public TicketController(CinemaManagementContext context) : base(context)
  {
    }

     public IActionResult Index()
        {
     return View();
        }

      [HttpGet]
        public IActionResult SelectSeats(Guid showtimeId)
{
   // Check if user is logged in
       var userId = HttpContext.Session.GetString("UserId");
      if (string.IsNullOrEmpty(userId))
    {
      // Redirect to login with return URL
    return RedirectToAction("Index", "Auth", new { returnUrl = Url.Action("SelectSeats", "Ticket", new { showtimeId }) });
    }

     var showTime = _context.ShowTimes
    .Include(s => s.Movie)
    .Include(s => s.Room)
    .ThenInclude(r => r.Cinema)
      .FirstOrDefault(s => s.ShowTimeId == showtimeId);

    if (showTime == null)
      return NotFound();

  // 🔥 Get all seats for this room with SeatStatus included
        var seats = _context.Seats
    .Include(s => s.SeatStatus)
    .Where(s => s.RoomId == showTime.RoomId)
  .OrderBy(s => s.RowLabel)
      .ThenBy(s => s.ColNumber)
.ToList();

     // 🔥 Get booked/held seats for this showtime
     // Check both ShowTimeSeats.Status AND Seat.SeatStatusId
     var bookedStatusId = Guid.Parse("550e8400-e29b-41d4-a716-000000000003"); // Booked
     var inactiveStatusId = Guid.Parse("550e8400-e29b-41d4-a716-000000000002"); // Inactive (holding)
     var activeStatusId = Guid.Parse("550e8400-e29b-41d4-a716-000000000001"); // Active (available)
     
     var bookedSeats = _context.ShowTimeSeats
          .Where(sts => sts.ShowTimeId == showtimeId && (sts.Status == 1 || sts.Status == 2))
   .Select(sts => sts.SeatId)
   .Union(_context.Seats
       .Where(s => s.RoomId == showTime.RoomId && 
               (s.SeatStatusId == bookedStatusId || s.SeatStatusId == inactiveStatusId))
       .Select(s => s.SeatId))
       .ToHashSet();
     
     // 🔥 Filter seats: only show those with Active or Booked/Inactive status
     seats = seats.Where(s => 
        s.SeatStatusId == activeStatusId || 
        s.SeatStatusId == bookedStatusId || 
        s.SeatStatusId == inactiveStatusId).ToList();

      var viewModel = new SelectSeatsViewModel
         {
   ShowTimeId = showtimeId,
   MovieTitle = showTime.Movie.Title,
    MoviePoster = showTime.Movie.PosterUrl,
        RoomName = showTime.Room.Name,
  CinemaName = showTime.Room.Cinema.Name,
ShowDate = showTime.StartAt,
   Format = showTime.Format ?? "2D PHỤ ĐỀ VIỆT",
       BasePrice = showTime.BasePrice,
   Seats = seats,
BooedSeatIds = bookedSeats
    };

      return View(viewModel);
     }
    }

    public class SelectSeatsViewModel
  {
   public Guid ShowTimeId { get; set; }
        public string MovieTitle { get; set; }
    public string MoviePoster { get; set; }
     public string RoomName { get; set; }
     public string CinemaName { get; set; }
        public DateTime ShowDate { get; set; }
     public string Format { get; set; }
public decimal BasePrice { get; set; }
   public List<Seat> Seats { get; set; }
   public HashSet<Guid> BooedSeatIds { get; set; }
    }
}
