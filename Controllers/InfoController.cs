using CinemaManagement.Data;
using CinemaManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers;

public class InfoController : BaseController
{
    public InfoController(CinemaManagementContext context) : base(context)
    {
    }

    public IActionResult CinemaSystem()
    {
        var items = _context.Cinemas
            .Include(c => c.Rooms)
            .Where(c => c.Status == 1)
            .OrderBy(c => c.Name)
            .Select(c => new CinemaSystemItemViewModel
            {
                CinemaName = c.Name,
                Address = c.Address,
                ActiveRooms = c.Rooms.Count(r => r.Status == 1),
                TotalSeats = c.Rooms.Where(r => r.Status == 1).Sum(r => r.TotalRows * r.TotalCols)
            })
            .ToList();

        return View(items);
    }

    public IActionResult TicketPrices()
    {
        var nowUtc = DateTime.UtcNow;

        var items = _context.ShowTimes
            .Include(st => st.Movie)
            .Include(st => st.Room)
            .ThenInclude(r => r.Cinema)
            .Where(st => st.Status == 1 && st.StartAt >= nowUtc && st.Room.Status == 1 && st.Room.Cinema.Status == 1)
            .OrderBy(st => st.StartAt)
            .Take(60)
            .Select(st => new TicketPriceItemViewModel
            {
                MovieTitle = st.Movie.Title,
                CinemaName = st.Room.Cinema.Name,
                RoomName = st.Room.Name,
                Format = st.Format ?? "2D PHỤ ĐỀ VIỆT",
                StartAt = st.StartAt,
                BasePrice = st.BasePrice
            })
            .ToList();

        return View(items);
    }

    public IActionResult NewsPromotions()
    {
        return View();
    }

    public IActionResult Franchise()
    {
        return View();
    }
}