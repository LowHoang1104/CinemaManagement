using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaManagement.Services
{
    public class ShowTimeService : IShowTimeService
    {
        private readonly CinemaManagementContext _context;

        public ShowTimeService(CinemaManagementContext context)
        {
            _context = context;
        }

        public async Task<ShowTimeListViewModel> GetShowTimeListAsync(
            string? search,
            DateTime? date,
            Guid? cinemaId,
            int? status,
            int? displayStatus,
            int page,
            int pageSize)
        {
            var nowUtc = DateTime.UtcNow;

            // --- Global stats (unfiltered) ---
            var stats = await _context.ShowTimes
                .Select(s => new { s.Status, s.StartAt, s.EndAt })
                .ToListAsync();

            // --- Filtered query ---
            IQueryable<ShowTime> query = _context.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Cinema)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.Movie.Title.Contains(search));

            if (date.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(date.Value, DateTimeKind.Local).ToUniversalTime();
                var endUtc = DateTime.SpecifyKind(date.Value.AddDays(1), DateTimeKind.Local).ToUniversalTime();
                query = query.Where(s => s.StartAt >= startUtc && s.StartAt < endUtc);
            }

            if (cinemaId.HasValue)
                query = query.Where(s => s.Room.Cinema.CinemaId == cinemaId.Value);

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            if (displayStatus.HasValue)
            {
                switch (displayStatus.Value)
                {
                    case 0: query = query.Where(s => s.Status == 0); break;
                    case 1: query = query.Where(s => s.Status == 1 && s.StartAt > nowUtc); break;
                    case 2: query = query.Where(s => s.Status == 1 && s.StartAt <= nowUtc && s.EndAt >= nowUtc); break;
                    case 3: query = query.Where(s => s.Status == 1 && s.EndAt < nowUtc); break;
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
                    ShowTimeId = s.ShowTimeId,
                    MovieTitle = s.Movie.Title,
                    CinemaName = s.Room.Cinema.Name,
                    RoomName = s.Room.Name,
                    StartAt = s.StartAt,
                    EndAt = s.EndAt,
                    CleaningEndsAt = s.EndAt.AddMinutes(15),
                    BasePrice = s.BasePrice,
                    Status = s.Status,
                    AgeRating = s.Movie.AgeRating
                })
                .ToListAsync();

            // Transform for UI (UTC -> Local)
            foreach (var st in showTimesData)
            {
                if (st.Status == 0) st.DisplayStatus = 0;
                else if (nowUtc < st.StartAt) st.DisplayStatus = 1;
                else if (nowUtc >= st.StartAt && nowUtc <= st.EndAt) st.DisplayStatus = 2;
                else st.DisplayStatus = 3;

                st.StartAt = st.StartAt.ToLocalTime();
                st.EndAt = st.EndAt.ToLocalTime();
                st.CleaningEndsAt = st.CleaningEndsAt.ToLocalTime();
            }

            var movies = await _context.Movies.ToListAsync();
            var rooms = await _context.Rooms.Include(r => r.Cinema).ToListAsync();
            var activeRooms = rooms.Where(r => r.Status == 1 && r.Cinema.Status == 1).ToList();
            var roomLocations = activeRooms.ToDictionary(
                r => r.RoomId.ToString().ToLower(),
                r => $"📍 {r.Cinema.Name} - {r.Name} - Standard"
            );

            return new ShowTimeListViewModel
            {
                ShowTimes = showTimesData,
                UpcomingCount = stats.Count(s => s.Status == 1 && s.StartAt > nowUtc),
                NowShowingCount = stats.Count(s => s.Status == 1 && s.StartAt <= nowUtc && s.EndAt >= nowUtc),
                CancelledCount = stats.Count(s => s.Status == 0),
                TodayCount = stats.Count(s => s.StartAt.ToLocalTime().Date == DateTime.Today),
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize,
                SearchTerm = search,
                DateFilter = date,
                CinemaIdFilter = cinemaId,
                StatusFilter = status,
                DisplayStatusFilter = displayStatus,
                MovieDurations = movies.ToDictionary(m => m.MovieId.ToString().ToLower(), m => m.DurationMin),
                RoomLocations = roomLocations,
                CreateForm = new ShowTimeCreateViewModel
                {
                    Movies = new SelectList(movies.Where(m => m.Status == 1).Select(m => new { m.MovieId, DisplayTitle = $"{m.Title} ({m.DurationMin} phút)" }), "MovieId", "DisplayTitle"),
                    Rooms = new SelectList(activeRooms, "RoomId", "Name", null, "Cinema.Name"),
                    StartAt = DateTime.Now.AddHours(1)
                },
                Cinemas = new SelectList(await _context.Cinemas.ToListAsync(), "CinemaId", "Name")
            };
        }

        public async Task<(bool IsOverlapping, List<object> Conflicts, bool IsPast)> CheckOverlapAsync(Guid roomId, DateTime startAt, Guid movieId, string? excludeId = null)
        {
            startAt = startAt.ToUniversalTime();
            if (startAt < DateTime.UtcNow.AddMinutes(-1))
                return (true, new List<object>(), true);

            Guid? excludeGuid = null;
            if (!string.IsNullOrEmpty(excludeId) && Guid.TryParse(excludeId, out var parsedGuid))
                excludeGuid = parsedGuid;

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null) throw new InvalidOperationException("Phim không tồn tại.");

            DateTime endAt = startAt.AddMinutes(movie.DurationMin);
            DateTime occupiedUntil = endAt.AddMinutes(15);
            DateTime startCheck = startAt.AddMinutes(-15);

            var roomShows = await _context.ShowTimes
                .Include(s => s.Movie)
                .Where(s => s.RoomId == roomId && s.Status == 1 && s.ShowTimeId != excludeGuid)
                .ToListAsync();

            var conflictingShows = roomShows
                .Where(s => startCheck < s.EndAt && s.StartAt < occupiedUntil)
                .Select(s => (object)new
                {
                    movieTitle = s.Movie.Title,
                    startTime = s.StartAt.ToLocalTime().ToString("HH:mm"),
                    endTime = s.EndAt.ToLocalTime().ToString("HH:mm"),
                    duration = s.Movie.DurationMin
                })
                .ToList();

            return (conflictingShows.Any(), conflictingShows, false);
        }

        public async Task CreateAsync(ShowTimeCreateViewModel model)
        {
            DateTime startAtUtc = model.StartAt.ToUniversalTime();
            if (startAtUtc < DateTime.UtcNow)
                throw new InvalidOperationException("Thời gian bắt đầu suất chiếu không thể nằm trong quá khứ.");

            var movie = await _context.Movies.FindAsync(model.MovieId)
                        ?? throw new InvalidOperationException("Phim không tồn tại.");

            if (movie.Status == 0)
                throw new InvalidOperationException("Phim này đã ngừng kinh doanh, không thể lên lịch chiếu.");

            var room = await _context.Rooms.Include(r => r.Cinema)
                        .FirstOrDefaultAsync(r => r.RoomId == model.RoomId)
                        ?? throw new InvalidOperationException("Phòng không tồn tại.");

            if (room.Status == 0)
                throw new InvalidOperationException("Phòng chiếu này đang bảo trì, không thể lên lịch chiếu.");

            if (room.Cinema.Status == 0)
                throw new InvalidOperationException("Rạp chiếu sở hữu phòng này đang ngừng hoạt động.");

            DateTime endAtUtc = startAtUtc.AddMinutes(movie.DurationMin);
            DateTime occupiedUntilUtc = endAtUtc.AddMinutes(15);
            DateTime startCheckUtc = startAtUtc.AddMinutes(-15);

            var overlappingShow = await _context.ShowTimes
                .Where(s => s.RoomId == model.RoomId && s.Status == 1)
                .AnyAsync(s => startCheckUtc < s.EndAt && s.StartAt < occupiedUntilUtc);

            if (overlappingShow)
                throw new InvalidOperationException("Phòng đã có lịch chiếu khác trong khoảng thời gian này (bao gồm 15 phút dọn dẹp).");

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
        }

        public async Task EditAsync(Guid id, ShowTimeEditViewModel model)
        {
            DateTime startAtUtc = model.StartAt.ToUniversalTime();
            if (startAtUtc < DateTime.UtcNow)
                throw new InvalidOperationException("Thời gian bắt đầu suất chiếu không thể nằm trong quá khứ.");

            var showTime = await _context.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Tickets)
                .Include(s => s.Room).ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(s => s.ShowTimeId == id)
                ?? throw new InvalidOperationException("Không tìm thấy suất chiếu.");

            if (showTime.Status == 0 || showTime.StartAt <= DateTime.UtcNow)
                throw new InvalidOperationException("Chỉ có thể sửa suất chiếu ở tương lai và chưa bị hủy.");

            // QUY TẮC NGHIÊM NGẶT: Nếu đã bán vé, không được đổi giờ/giá
            if (showTime.Tickets.Any())
                throw new InvalidOperationException("Suất chiếu đã bán vé, không thể chỉnh sửa.");

            if (showTime.Movie.Status == 0)
                throw new InvalidOperationException("Phim này đã ngừng kinh doanh.");

            if (showTime.Room.Status == 0)
                throw new InvalidOperationException("Phòng chiếu này đang bảo trì.");

            if (showTime.Room.Cinema.Status == 0)
                throw new InvalidOperationException("Rạp chiếu sở hữu phòng này đang ngừng hoạt động.");

            DateTime endAtUtc = startAtUtc.AddMinutes(showTime.Movie.DurationMin);
            DateTime occupiedUntilUtc = endAtUtc.AddMinutes(15);
            DateTime startCheckUtc = startAtUtc.AddMinutes(-15);

            var overlappingShow = await _context.ShowTimes
                .Where(s => s.RoomId == showTime.RoomId && s.ShowTimeId != id && s.Status == 1)
                .AnyAsync(s => startCheckUtc < s.EndAt && s.StartAt < occupiedUntilUtc);

            if (overlappingShow)
                throw new InvalidOperationException("Phòng đã có lịch chiếu khác trong khoảng thời gian này (bao gồm 15 phút dọn dẹp).");

            showTime.StartAt = startAtUtc;
            showTime.EndAt = endAtUtc;
            showTime.BasePrice = model.BasePrice;
            showTime.LastUpdatedAt = DateTime.UtcNow;

            _context.Update(showTime);
            await _context.SaveChangesAsync();
        }

        public async Task CancelAsync(Guid id)
        {
            var showTime = await _context.ShowTimes
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.ShowTimeId == id)
                ?? throw new InvalidOperationException("Không tìm thấy suất chiếu.");

            if (showTime.Status == 0 || showTime.StartAt <= DateTime.UtcNow)
                throw new InvalidOperationException("Không thể hủy suất chiếu đang chiếu, đã kết thúc hoặc đã bị hủy.");

            if (showTime.Tickets.Any())
                throw new InvalidOperationException("Không thể hủy suất chiếu đã có vé được bán.");

            showTime.Status = 0;
            showTime.LastUpdatedAt = DateTime.UtcNow;
            _context.Update(showTime);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var showTime = await _context.ShowTimes.FindAsync(id)
                          ?? throw new InvalidOperationException("Không tìm thấy suất chiếu.");

            var hasTickets = await _context.Tickets.AnyAsync(t => t.ShowTimeId == id);
            if (hasTickets)
                throw new InvalidOperationException("Không thể xóa lịch chiếu đã có vé được bán.");

            _context.ShowTimes.Remove(showTime);
            await _context.SaveChangesAsync();
        }

        public async Task<ShowTimeDetailViewModel?> GetDetailsAsync(Guid id)
        {
            var showTime = await _context.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(s => s.ShowTimeId == id);

            if (showTime == null) return null;

            return new ShowTimeDetailViewModel
            {
                ShowTimeId = showTime.ShowTimeId,
                MovieTitle = showTime.Movie.Title,
                MovieDuration = showTime.Movie.DurationMin,
                AgeRating = showTime.Movie.AgeRating,
                RoomName = showTime.Room.Name,
                CinemaName = showTime.Room.Cinema.Name,
                StartAt = showTime.StartAt.ToLocalTime(),
                EndAt = showTime.EndAt.ToLocalTime(),
                CleaningEndsAt = showTime.EndAt.ToLocalTime().AddMinutes(15),
                CreatedAt = showTime.CreatedAt.ToLocalTime(),
                BasePrice = showTime.BasePrice,
                Status = showTime.Status,
                DisplayStatus = showTime.Status == 0 ? 0 :
                               (DateTime.UtcNow < showTime.StartAt ? 1 :
                               (DateTime.UtcNow <= showTime.EndAt ? 2 : 3))
            };
        }

        public async Task<ShowTimeEditViewModel?> GetEditViewModelAsync(Guid id)
        {
            var showTime = await _context.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(s => s.ShowTimeId == id);

            if (showTime == null) return null;

            return new ShowTimeEditViewModel
            {
                ShowTimeId = showTime.ShowTimeId,
                MovieId = showTime.MovieId,
                RoomId = showTime.RoomId,
                StartAt = showTime.StartAt.ToLocalTime(),
                BasePrice = showTime.BasePrice,
                MovieTitle = showTime.Movie.Title,
                MovieDuration = showTime.Movie.DurationMin,
                RoomName = showTime.Room.Name,
                CinemaName = showTime.Room.Cinema.Name
            };
        }
    }
}
