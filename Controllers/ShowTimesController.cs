using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;

namespace CinemaManagement.Controllers;

public class ShowTimesController : Controller
{
    private readonly CinemaManagementContext _context;

    public ShowTimesController(CinemaManagementContext context)
    {
        _context = context;
    }

    // GET: ShowTimes
    public async Task<IActionResult> Index(
        string? search,
        DateTime? date,
        Guid? cinemaId,
        int? status,
        int? displayStatus,
        int page = 1,
        int pageSize = 5)
    {

        // --- Global stats (unfiltered, always full counts for dashboard) ---
        var stats = await _context.ShowTimes
            .Select(s => new { s.Status, s.StartAt, s.EndAt })
            .ToListAsync();

        // --- Cinemas for filter dropdown ---
        var cinemas = await _context.Cinemas.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Cinemas = new SelectList(cinemas, "CinemaId", "Name", cinemaId);

        // --- Filtered, paginated query ---
        IQueryable<ShowTime> query = _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
                .ThenInclude(r => r.Cinema)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Movie.Title.Contains(search));

        if (date.HasValue)
        {
            // Compare against UTC stored values: convert the local date boundary to UTC
            var startUtc = DateTime.SpecifyKind(date.Value, DateTimeKind.Local).ToUniversalTime();
            var endUtc   = DateTime.SpecifyKind(date.Value.AddDays(1), DateTimeKind.Local).ToUniversalTime();
            query = query.Where(s => s.StartAt >= startUtc && s.StartAt < endUtc);
        }

        if (cinemaId.HasValue)
            query = query.Where(s => s.Room.Cinema.CinemaId == cinemaId.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var nowUtc = DateTime.UtcNow;
        if (displayStatus.HasValue)
        {
            switch (displayStatus.Value)
            {
                case 0: // Cancelled
                    query = query.Where(s => s.Status == 0);
                    break;
                case 1: // Upcoming
                    query = query.Where(s => s.Status == 1 && s.StartAt > nowUtc);
                    break;
                case 2: // Now Showing
                    query = query.Where(s => s.Status == 1 && s.StartAt <= nowUtc && s.EndAt >= nowUtc);
                    break;
                case 3: // Ended
                    query = query.Where(s => s.Status == 1 && s.EndAt < nowUtc);
                    break;
            }
        }

        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

        var showTimesData = await query
            .OrderByDescending(s => s.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ShowTimeIndexViewModel
            {
                ShowTimeId     = s.ShowTimeId,
                MovieTitle     = s.Movie.Title,
                CinemaName     = s.Room.Cinema.Name,
                RoomName       = s.Room.Name,
                StartAt        = s.StartAt,
                EndAt          = s.EndAt,
                CleaningEndsAt = s.EndAt.AddMinutes(15),
                BasePrice      = s.BasePrice,
                Status         = s.Status,
                AgeRating      = s.Movie.AgeRating
            })
            .ToListAsync();

        // Convert UTC → local time and calculate DisplayStatus
        foreach (var st in showTimesData)
        {
            if (st.Status == 0) st.DisplayStatus = 0; // Cancelled
            else if (nowUtc < st.StartAt) st.DisplayStatus = 1; // Upcoming
            else if (nowUtc >= st.StartAt && nowUtc <= st.EndAt) st.DisplayStatus = 2; // Now Showing
            else st.DisplayStatus = 3; // Ended

            st.StartAt        = st.StartAt.ToLocalTime();
            st.EndAt          = st.EndAt.ToLocalTime();
            st.CleaningEndsAt = st.CleaningEndsAt.ToLocalTime();
        }

        // --- Data for Create modal dropdowns ---
        var movies      = await _context.Movies.ToListAsync();
        var rooms       = await _context.Rooms.Include(r => r.Cinema).ToListAsync();
        var activeRooms = rooms.Where(r => r.Status == 1).ToList();
        var roomLocations = activeRooms.ToDictionary(
            r => r.RoomId.ToString().ToLower(),
            r => $"📍 {r.Cinema.Name} - {r.Name} - Standard"
        );

        var viewModel = new ShowTimeListViewModel
        {
            ShowTimes      = showTimesData,
            UpcomingCount  = stats.Count(s => s.Status == 1 && s.StartAt > nowUtc),
            NowShowingCount = stats.Count(s => s.Status == 1 && s.StartAt <= nowUtc && s.EndAt >= nowUtc),
            CancelledCount = stats.Count(s => s.Status == 0),
            TodayCount     = stats.Count(s => s.StartAt.ToLocalTime().Date == DateTime.Today),

            // Pagination state
            CurrentPage = page,
            TotalPages  = totalPages,
            TotalItems  = totalItems,
            PageSize    = pageSize,

            // Active filter state (to pre-fill form)
            SearchTerm    = search,
            DateFilter    = date,
            CinemaIdFilter = cinemaId,
            StatusFilter  = status,
            DisplayStatusFilter = displayStatus,

            MovieDurations = movies.ToDictionary(m => m.MovieId.ToString().ToLower(), m => m.DurationMin),
            RoomLocations  = roomLocations,
            CreateForm = new ShowTimeCreateViewModel
            {
                Movies  = new SelectList(movies.Where(m => m.Status == 1).Select(m => new { m.MovieId, DisplayTitle = $"{m.Title} ({m.DurationMin} phút)" }), "MovieId", "DisplayTitle"),
                Rooms   = new SelectList(activeRooms, "RoomId", "Name", null, "Cinema.Name"),
                StartAt = DateTime.Now.AddHours(1)
            }
        };

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_ShowTimeListPartial", viewModel);
        }

        return View(viewModel);
    }

    // Lightweight helper used only by Create POST failure path (no filters)
    private async Task<ShowTimeListViewModel> GetShowTimeListViewModel()
    {

        var stats = await _context.ShowTimes
            .Select(s => new { s.Status, s.StartAt, s.EndAt })
            .ToListAsync();

        var nowUtc = DateTime.UtcNow;

        var showTimesData = await _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
                .ThenInclude(r => r.Cinema)
            .OrderByDescending(s => s.StartAt)
            .Select(s => new ShowTimeIndexViewModel
            {
                ShowTimeId     = s.ShowTimeId,
                MovieTitle     = s.Movie.Title,
                CinemaName     = s.Room.Cinema.Name,
                RoomName       = s.Room.Name,
                StartAt        = s.StartAt,
                EndAt          = s.EndAt,
                CleaningEndsAt = s.EndAt.AddMinutes(15),
                BasePrice      = s.BasePrice,
                Status         = s.Status,
                AgeRating      = s.Movie.AgeRating
            })
            .ToListAsync();

        var movies      = await _context.Movies.ToListAsync();
        var rooms       = await _context.Rooms.Include(r => r.Cinema).ToListAsync();
        var activeRooms = rooms.Where(r => r.Status == 1).ToList();
        var roomLocations = activeRooms.ToDictionary(
            r => r.RoomId.ToString().ToLower(),
            r => $"📍 {r.Cinema.Name} - {r.Name} - Standard"
        );

        foreach (var st in showTimesData)
        {
            if (st.Status == 0) st.DisplayStatus = 0;
            else if (nowUtc < st.StartAt) st.DisplayStatus = 1;
            else if (nowUtc >= st.StartAt && nowUtc <= st.EndAt) st.DisplayStatus = 2;
            else st.DisplayStatus = 3;

            st.StartAt        = st.StartAt.ToLocalTime();
            st.EndAt          = st.EndAt.ToLocalTime();
            st.CleaningEndsAt = st.CleaningEndsAt.ToLocalTime();
        }

        return new ShowTimeListViewModel
        {
            ShowTimes      = showTimesData,
            UpcomingCount  = stats.Count(s => s.Status == 1 && s.StartAt > nowUtc),
            NowShowingCount = stats.Count(s => s.Status == 1 && s.StartAt <= nowUtc && s.EndAt >= nowUtc),
            CancelledCount = stats.Count(s => s.Status == 0),
            TodayCount     = stats.Count(s => s.StartAt.ToLocalTime().Date == DateTime.Today),
            TotalItems     = stats.Count,
            TotalPages     = 1,
            MovieDurations = movies.ToDictionary(m => m.MovieId.ToString().ToLower(), m => m.DurationMin),
            RoomLocations  = roomLocations,
            CreateForm = new ShowTimeCreateViewModel
            {
                Movies  = new SelectList(movies.Where(m => m.Status == 1).Select(m => new { m.MovieId, DisplayTitle = $"{m.Title} ({m.DurationMin} phút)" }), "MovieId", "DisplayTitle"),
                Rooms   = new SelectList(activeRooms, "RoomId", "Name", null, "Cinema.Name"),
                StartAt = DateTime.Now.AddHours(1)
            }
        };
    }

    // GET: ShowTimes/Create
    public async Task<IActionResult> Create()
    {
        var movies = await _context.Movies.Where(m => m.Status == 1).ToListAsync();
        var rooms = await _context.Rooms.Where(r => r.Status == 1).ToListAsync();

        ViewBag.MovieDurations = movies.ToDictionary(m => m.MovieId.ToString(), m => m.DurationMin);

        var viewModel = new ShowTimeCreateViewModel
        {
            StartAt = DateTime.Now.AddHours(1), // Default to 1 hour from now in Local Time
            Movies = new SelectList(movies.Select(m => new { m.MovieId, DisplayTitle = $"{m.Title} ({m.DurationMin} phút)" }), "MovieId", "DisplayTitle"),
            Rooms = new SelectList(rooms, "RoomId", "Name")
        };
        return View(viewModel);
    }

    // POST: ShowTimes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "CreateForm")] ShowTimeCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Convert to UTC for DB operations WITHOUT mutating model.StartAt
            // (so that if validation fails, the form can re-populate with the user's original local time)
            DateTime startAtUtc = model.StartAt.Kind == DateTimeKind.Utc
                ? model.StartAt
                : model.StartAt.ToUniversalTime();

            if (startAtUtc < DateTime.UtcNow)
            {
                ModelState.AddModelError("CreateForm.StartAt", "Thời gian bắt đầu suất chiếu không thể nằm trong quá khứ.");
            }
            else
            {
                var movie = await _context.Movies.FindAsync(model.MovieId);
                if (movie == null)
                {
                    ModelState.AddModelError("CreateForm.MovieId", "Phim không tồn tại.");
                }
            else
            {
                DateTime endAtUtc = startAtUtc.AddMinutes(movie.DurationMin);
                DateTime occupiedUntilUtc = endAtUtc.AddMinutes(15);
                DateTime startCheckUtc = startAtUtc.AddMinutes(-15);
                
                var overlappingShow = await _context.ShowTimes
                    .Where(s => s.RoomId == model.RoomId && s.Status == 1)
                    .AnyAsync(s => startCheckUtc < s.EndAt && s.StartAt < occupiedUntilUtc);

                if (overlappingShow)
                {
                    ModelState.AddModelError("", "Phòng đã có lịch chiếu khác trong khoảng thời gian này (bao gồm 15 phút dọn dẹp).");
                }
                else
                {
                    var showTime = new ShowTime
                    {
                        ShowTimeId = Guid.NewGuid(),
                        MovieId = model.MovieId,
                        RoomId = model.RoomId,
                        StartAt = startAtUtc,
                        EndAt = endAtUtc,
                        BasePrice = model.BasePrice,
                        Status = 1,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Add(showTime);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Suất chiếu đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
        }
        }

        // FAILURE: Re-render Index with errors
        var viewModel = await GetShowTimeListViewModel();
        viewModel.CreateForm = model; // Keep the posted data
        
        // Re-populate dropdowns for the posted form
        var activeMovies = await _context.Movies.Where(m => m.Status == 1).ToListAsync();
        var activeRoomsList = await _context.Rooms.Include(r => r.Cinema).Where(r => r.Status == 1).ToListAsync();
        viewModel.CreateForm.Movies = new SelectList(activeMovies, "MovieId", "Title", model.MovieId);
        viewModel.CreateForm.Rooms = new SelectList(activeRoomsList, "RoomId", "Name", model.RoomId, "Cinema.Name");
        
        return View("Index", viewModel);
    }
    
    // GET: ShowTimes/CheckOverlap?roomId=...&startAt=...&movieId=...&excludeId=...
    [HttpGet]
    public async Task<IActionResult> CheckOverlap(Guid roomId, DateTime startAt, Guid movieId, string? excludeId = null)
    {
        // Convert to UTC immediately to prevent Npgsql DateTimeKind.Unspecified vs UTC errors
        startAt = startAt.ToUniversalTime();

        // Prevent creating showtimes in the past (allow a 1-minute buffer for form submission time)
        // Note: DateTime.UtcNow is already UTC, so comparison is safe.
        if (startAt < DateTime.UtcNow.AddMinutes(-1)) 
        {
            return Ok(new { isOverlapping = true, isPast = true, message = "Không thể chọn thời gian trong quá khứ" });
        }

        // SQL Server datetime min value is 1753-01-01. Prevent 500 errors from partial year typing.
        if (startAt < new DateTime(1753, 1, 1)) return Ok(new { isOverlapping = false, message = "Ngày không hợp lệ" });

        Guid? excludeGuid = null;
        if (!string.IsNullOrEmpty(excludeId) && Guid.TryParse(excludeId, out var parsedGuid))
        {
            excludeGuid = parsedGuid;
        }
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null) return NotFound(new { message = "Không tìm thấy phim." });

        DateTime endAt = startAt.AddMinutes(movie.DurationMin);
        DateTime occupiedUntil = endAt.AddMinutes(15);
        DateTime startCheck = startAt.AddMinutes(-15);

        // Fetch room showtimes first, then run date math in memory avoiding EF Core DB Translation errors
        var roomShows = await _context.ShowTimes
            .Include(s => s.Movie)
            .Where(s => s.RoomId == roomId && s.Status == 1 && s.ShowTimeId != excludeGuid)
            .ToListAsync();

        var conflictingShows = roomShows
            .Where(s => startCheck < s.EndAt && s.StartAt < occupiedUntil)
            .Select(s => new 
            {
                movieTitle = s.Movie.Title,
                startTime = s.StartAt.ToLocalTime().ToString("HH:mm"),
                endTime = s.EndAt.ToLocalTime().ToString("HH:mm"),
                duration = s.Movie.DurationMin
            })
            .ToList();

        var isOverlapping = conflictingShows.Any();

        return Ok(new { isOverlapping, conflicts = conflictingShows });
    }

    // GET: ShowTimes/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var showTime = await _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
                .ThenInclude(r => r.Cinema)
            .FirstOrDefaultAsync(s => s.ShowTimeId == id);

        if (showTime == null) return NotFound();

        var startAtLocal = showTime.StartAt.ToLocalTime();
        var endAtLocal = showTime.EndAt.ToLocalTime();

        var vm = new ShowTimeDetailViewModel
        {
            ShowTimeId  = showTime.ShowTimeId,
            MovieTitle  = showTime.Movie.Title,
            MovieDuration = showTime.Movie.DurationMin,
            AgeRating   = showTime.Movie.AgeRating,
            RoomName    = showTime.Room.Name,
            CinemaName  = showTime.Room.Cinema.Name,
            StartAt     = startAtLocal,
            EndAt       = endAtLocal,
            CleaningEndsAt = endAtLocal.AddMinutes(15),
            CreatedAt   = showTime.CreatedAt.ToLocalTime(),
            BasePrice   = showTime.BasePrice,
            Status      = showTime.Status,
            DisplayStatus = showTime.Status == 0 ? 0 :
                           (DateTime.UtcNow < showTime.StartAt ? 1 :
                           (DateTime.UtcNow <= showTime.EndAt ? 2 : 3))
        };

        return PartialView("_DetailsModalPartial", vm);
    }

    // GET: ShowTimes/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var showTime = await _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
                .ThenInclude(r => r.Cinema)
            .FirstOrDefaultAsync(s => s.ShowTimeId == id);

        if (showTime == null) return NotFound();

        // Security check: Only allow editing "Upcoming" showtimes
        if (showTime.Status == 0 || showTime.StartAt <= DateTime.UtcNow)
        {
            return BadRequest("Chỉ có thể sửa suất chiếu khi chưa bắt đầu.");
        }

        var viewModel = new ShowTimeEditViewModel
        {
            ShowTimeId = showTime.ShowTimeId,
            MovieId = showTime.MovieId,
            RoomId = showTime.RoomId,
            StartAt = showTime.StartAt.ToLocalTime(), // Convert UTC from DB back to Local time for the form
            BasePrice = showTime.BasePrice,
            MovieTitle = showTime.Movie.Title,
            MovieDuration = showTime.Movie.DurationMin,
            RoomName = showTime.Room.Name,
            CinemaName = showTime.Room.Cinema.Name
        };

        return PartialView("_EditModalPartial", viewModel);
    }

    // POST: ShowTimes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ShowTimeEditViewModel model)
    {
        if (id != model.ShowTimeId) return BadRequest();

        if (ModelState.IsValid)
        {
            // Convert to UTC for DB operations WITHOUT mutating model.StartAt
            DateTime startAtUtc = model.StartAt.Kind == DateTimeKind.Utc
                ? model.StartAt
                : model.StartAt.ToUniversalTime();

            if (startAtUtc < DateTime.UtcNow)
            {
                ModelState.AddModelError("StartAt", "Thời gian bắt đầu suất chiếu không thể nằm trong quá khứ.");
            }
            else
            {
                // HARDENING: Always use RoomId and MovieId from the verified database record.
                // These cannot be changed after creation.
                var showTime = await _context.ShowTimes
                    .Include(s => s.Movie)
                    .FirstOrDefaultAsync(s => s.ShowTimeId == id);

                if (showTime == null) return NotFound();

                // Security check: Only allow editing "Upcoming" showtimes
                if (showTime.Status == 0 || showTime.StartAt <= DateTime.UtcNow)
                {
                    return BadRequest("Chỉ có thể sửa suất chiếu khi chưa bắt đầu.");
                }

                // Perform calculations and checks using DB-verified IDs
                DateTime endAtUtc = startAtUtc.AddMinutes(showTime.Movie.DurationMin);
                DateTime occupiedUntilUtc = endAtUtc.AddMinutes(15);
                DateTime startCheckUtc = startAtUtc.AddMinutes(-15);

                var roomShows = await _context.ShowTimes
                    .Where(s => s.RoomId == showTime.RoomId && s.ShowTimeId != id && s.Status == 1)
                    .ToListAsync();

                var overlappingShow = roomShows.Any(s => startCheckUtc < s.EndAt && s.StartAt < occupiedUntilUtc);

                if (overlappingShow)
                {
                    ModelState.AddModelError("", "Phòng đã có lịch chiếu khác trong khoảng thời gian này (bao gồm 15 phút dọn dẹp).");
                }
                else
                {
                    showTime.StartAt = startAtUtc;
                    showTime.EndAt = endAtUtc;
                    showTime.BasePrice = model.BasePrice;
                    showTime.LastUpdatedAt = DateTime.UtcNow;

                    _context.Update(showTime);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Suất chiếu đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        // Failure: Reload data for display-only fields before returning
        var dbShowTime = await _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
                .ThenInclude(r => r.Cinema)
            .FirstOrDefaultAsync(s => s.ShowTimeId == id);

        if (dbShowTime != null)
        {
            model.MovieId = dbShowTime.MovieId;
            model.RoomId = dbShowTime.RoomId;
            model.MovieTitle = dbShowTime.Movie.Title;
            model.MovieDuration = dbShowTime.Movie.DurationMin;
            model.RoomName = dbShowTime.Room.Name;
            model.CinemaName = dbShowTime.Room.Cinema.Name;
        }

        return PartialView("_EditModalPartial", model);
    }

    // POST: ShowTimes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var showTime = await _context.ShowTimes.FindAsync(id);
        if (showTime != null)
        {
            // Optionally check if tickets are already sold before deleting
            var hasTickets = await _context.Tickets.AnyAsync(t => t.ShowTimeId == id);
            if (hasTickets)
            {
                TempData["ErrorMessage"] = "Không thể xóa lịch chiếu đã có vé được bán.";
                return RedirectToAction(nameof(Index));
            }

            _context.ShowTimes.Remove(showTime);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa lịch chiếu thành công.";
        }
        return RedirectToAction(nameof(Index));
    }
    // POST: ShowTimes/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var showTime = await _context.ShowTimes
            .Include(s => s.Tickets)
            .FirstOrDefaultAsync(s => s.ShowTimeId == id);

        if (showTime == null) return NotFound(new { message = "Không tìm thấy suất chiếu." });

        if (showTime.Status == 0 || showTime.StartAt <= DateTime.UtcNow)
        {
            return BadRequest(new { message = "Không thể hủy suất chiếu đang chiếu, đã kết thúc hoặc đã bị hủy." });
        }

        if (showTime.Tickets.Any())
        {
            return BadRequest(new { message = "Cannot cancel. There are booked tickets. Please refund them first." });
        }

        showTime.Status = 0; // Cancelled
        showTime.LastUpdatedAt = DateTime.UtcNow;
        _context.Update(showTime);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Suất chiếu đã được hủy thành công." });
    }
}
