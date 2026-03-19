using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels.Cinema;
using System.Security.Claims;

namespace CinemaManagement.Controllers
{
    public class RoomsController : Controller
    {
        private readonly CinemaManagementContext _context;

        public RoomsController(CinemaManagementContext context)
        {
            _context = context;
        }

        // GET: Rooms
        public async Task<IActionResult> Index(Guid? cinemaId, string? search, RoomStatus? status,
                                               string? sortBy, string? sortDir, int page = 1, int pageSize = 3)
        {
            // 1. Base query with Eager Loading
            IQueryable<Room> baseQuery = _context.Rooms
                .Include(r => r.Cinema)
                .AsNoTracking();

            // 2. Stats
            int totalRooms = await baseQuery.CountAsync();
            int totalActiveRooms = await baseQuery.CountAsync(r => r.Status == (int)RoomStatus.Active);
            int totalInactiveRooms = await baseQuery.CountAsync(r => r.Status == (int)RoomStatus.Inactive);
            int totalSeats = totalRooms == 0 ? 0 : await baseQuery.SumAsync(r => r.TotalRows * r.TotalCols);

            // 3. Filtering
            IQueryable<Room> roomsQuery = baseQuery;
            if (cinemaId.HasValue) roomsQuery = roomsQuery.Where(r => r.CinemaId == cinemaId.Value);
            if (status.HasValue) roomsQuery = roomsQuery.Where(r => r.Status == (int)status.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim().ToLower();
                roomsQuery = roomsQuery.Where(r => r.Name.ToLower().Contains(keyword) || (r.Cinema != null && r.Cinema.Name.ToLower().Contains(keyword)));
            }

            // 4. Sorting (Fixed Stable Sorting)
            bool isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            roomsQuery = sortBy?.ToLower() switch
            {
                "name"   => isDesc 
                            ? roomsQuery.OrderByDescending(r => r.Name).ThenByDescending(r => r.RoomId) 
                            : roomsQuery.OrderBy(r => r.Name).ThenBy(r => r.RoomId),

                "cinema" => isDesc 
                            ? roomsQuery.OrderByDescending(r => r.Cinema.Name).ThenByDescending(r => r.Name).ThenByDescending(r => r.RoomId) 
                            : roomsQuery.OrderBy(r => r.Cinema.Name).ThenBy(r => r.Name).ThenBy(r => r.RoomId),

                "seats"  => isDesc 
                            ? roomsQuery.OrderByDescending(r => r.TotalRows * r.TotalCols).ThenByDescending(r => r.RoomId) 
                            : roomsQuery.OrderBy(r => r.TotalRows * r.TotalCols).ThenBy(r => r.RoomId),

                "date"   => isDesc 
                            ? roomsQuery.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.RoomId) 
                            : roomsQuery.OrderBy(r => r.CreatedAt).ThenBy(r => r.RoomId),

                // Mặc định: Rạp (A-Z) -> Phòng (A-Z) -> ID (cố định)
                _        => roomsQuery.OrderBy(r => r.Cinema.Name).ThenBy(r => r.Name).ThenBy(r => r.RoomId)
            };

            // 5. Pagination
            int totalItems = await roomsQuery.CountAsync();
            var rooms = await roomsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // 6. Build ViewModel
            var viewModel = new CinemaManagement.ViewModels.RoomListViewModel
            {
                Rooms = rooms,
                TotalRooms = totalRooms,
                TotalActiveRooms = totalActiveRooms,
                TotalInactiveRooms = totalInactiveRooms,
                TotalSeats = totalSeats,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                SearchTerm = search,
                CinemaIdFilter = cinemaId,
                StatusFilter = status,
                SortBy = sortBy,
                SortDir = sortDir
            };

            // 7. SelectList for filter
            var cinemas = await _context.Cinemas.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            ViewData["CinemaSelectList"] = new SelectList(cinemas, "CinemaId", "Name", cinemaId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RoomListPartial", viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id, int status)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phòng chiếu." });
            }

            // Ràng buộc: Nếu muốn chuyển sang Ngừng hoạt động (status = 0)
            if (status == 0)
            {
                // Kiểm tra xem phòng có suất chiếu nào ĐANG CHIẾU hoặc SẮP CHIẾU không
                // (Status = 1 và thời gian kết thúc vẫn ở trong tương lai)
                var hasActiveShowtimes = await _context.ShowTimes
                    .AnyAsync(s => s.RoomId == id && s.Status == 1 && s.EndAt >= DateTime.UtcNow);

                if (hasActiveShowtimes)
                {
                    return Json(new { success = false, message = "Không thể ngừng hoạt động! Phòng đang có lịch chiếu chưa kết thúc." });
                }
            }

            // Đổi trạng thái (1 = Hoạt động, 0 = Ngừng hoạt động)
            room.Status = status;

            try
            {
                _context.Update(room);
                await _context.SaveChangesAsync();
                
                string statusText = status == 1 ? "kích hoạt" : "ngừng hoạt động";
                return Json(new { 
                    success = true, 
                    message = $"Đã {statusText} phòng '{room.Name}' thành công.",
                    newStatus = status 
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi lưu trạng thái." });
            }
        }

        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Cinema)
                .FirstOrDefaultAsync(m => m.RoomId == id);
            
            if (room == null)
            {
                return NotFound();
            }

            // Truyền sang View
            ViewData["CinemaId"] = room.CinemaId;
            ViewData["CinemaName"] = room.Cinema?.Name;
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditModalPartial", room);
            }
            return View(room);
        }

        // POST: Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // BIND CHUẨN XÁC: CHỈ CHO CẬP NHẬT RoomId và Name (BRD Rule 3.3)
        public async Task<IActionResult> Edit(Guid id, [Bind("RoomId,Name")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            var existingRoom = await _context.Rooms.FindAsync(id);
            if (existingRoom == null)
            {
                return NotFound();
            }

            // Validate logic: Name phải unique trong cùng 1 Cinema
            bool isRoomNameExists = await _context.Rooms
                .AnyAsync(r => r.CinemaId == existingRoom.CinemaId 
                            && r.RoomId != id 
                            && r.Name.Trim().ToLower() == room.Name.Trim().ToLower());

            if (isRoomNameExists)
            {
                ModelState.AddModelError("Name", "Tên phòng chiếu này đã tồn tại trong rạp.");
            }

            // Bỏ qua validate các trường không Bind (Do entity cấu hình required)
            ModelState.Remove("Cinema");
            ModelState.Remove("Seats");
            ModelState.Remove("ShowTimes");
            ModelState.Remove("SeatCode");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only allowed fields
                    existingRoom.Name = room.Name;
                    existingRoom.LastUpdatedAt = DateTime.UtcNow;
                    // existingRoom.LastUpdatedBy = ... (Admin user ID)

                    _context.Update(existingRoom);
                    await _context.SaveChangesAsync();
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = $"Đã cập nhật tên phòng thành '{existingRoom.Name}'.", updatedName = existingRoom.Name, roomId = existingRoom.RoomId });
                    }

                    TempData["Success"] = $"Đã cập nhật tên phòng thành '{existingRoom.Name}'.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.RoomId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Nạp lại data cho view nếu có lỗi validate
            await _context.Entry(existingRoom).Reference(r => r.Cinema).LoadAsync();
            ViewData["CinemaId"] = existingRoom.CinemaId;
            ViewData["CinemaName"] = existingRoom.Cinema?.Name;
            
            // Revert changes on the tracked entity so we show the inputted name to fix
            existingRoom.Name = room.Name;
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditModalPartial", existingRoom);
            }
            return View(existingRoom);
        }

        private bool RoomExists(Guid id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }

        // GET: Rooms/Create
        public async Task<IActionResult> Create()
        {
            var cinemas = await _context.Cinemas
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewData["CinemaId"] = new SelectList(cinemas, "CinemaId", "Name");
            return View();
        }

        // POST: Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CinemaId,Name,TotalRows,TotalCols")] Room room)
        {
            // 1. Bỏ qua validate các navigation property không được Bind
            // (Cinema = null! khiến ModelState fail nếu không Remove)
            ModelState.Remove("Cinema");
            ModelState.Remove("Seats");
            ModelState.Remove("ShowTimes");

            // 2. Validate: Kiểm tra xem Tên phòng đã tồn tại trong Rạp này chưa
            bool isRoomNameExists = await _context.Rooms
                .AnyAsync(r => r.CinemaId == room.CinemaId && r.Name.Trim().ToLower() == room.Name.Trim().ToLower());

            if (isRoomNameExists)
            {
                ModelState.AddModelError("Name", "Tên phòng chiếu này đã tồn tại trong rạp được chọn.");
            }

            // 3. Kiểm tra tính hợp lệ của Model
            if (ModelState.IsValid)
            {
                // Bắt đầu Transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // A. Khởi tạo dữ liệu cho Room
                    var adminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    Guid? adminId = Guid.TryParse(adminIdStr, out var g) ? g : null;

                    room.RoomId = Guid.NewGuid();
                    room.Status = (int)RoomStatus.Active;
                    room.CreatedAt = DateTime.UtcNow;
                    room.CreatedBy = adminId;

                    _context.Rooms.Add(room);

                    // B. Vòng lặp sinh Ghế (Seats) tự động
                    var seatsToInsert = new List<Seat>();

                    for (int r = 1; r <= room.TotalRows; r++)
                    {
                        string rowLetter = GetRowLetter(r); // Convert 1 -> A, 2 -> B...

                        for (int c = 1; c <= room.TotalCols; c++)
                        {
                            seatsToInsert.Add(new Seat
                            {
                                SeatId = Guid.NewGuid(),
                                RoomId = room.RoomId,
                                SeatCode = $"{rowLetter}{c}", // VD: A1, A2, B10...
                                RowLabel = r,
                                ColNumber = c,
                                SeatType = SeatTypeEnum.Standard.ToString(), // Mặc định là ghế Standard
                                SeatStatusId = SeatStatusConstants.Active,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = adminId
                            });
                        }
                    }

                    // C. Insert hàng loạt ghế vào Database
                    await _context.Seats.AddRangeAsync(seatsToInsert);

                    // D. Lưu và Commit Transaction
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var msg = $"Đã tạo phòng '{room.Name}' và sinh tự động {room.TotalRows * room.TotalCols} ghế thành công!";

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = msg, redirectUrl = Url.Action(nameof(Index)) });
                    }

                    TempData["Success"] = msg;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Nếu có bất kỳ lỗi gì (đứt cáp, lỗi DB, lỗi logic...), Rollback toàn bộ!
                    await transaction.RollbackAsync();

                    var errorMsg = "Đã xảy ra lỗi hệ thống khi tạo phòng và ghế. Vui lòng thử lại.";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMsg + " Chi tiết: " + ex.Message });
                    }

                    ModelState.AddModelError("", errorMsg);
                    // Log.Error(ex, "Lỗi tạo phòng"); // Khuyên dùng thư viện Serilog/NLog để ghi log
                }
            }

            // Nếu code chạy đến đây nghĩa là có lỗi Validate hoặc lỗi Exception
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            // Cần Load lại Dropdown rạp chiếu để hiển thị lại View
            var cinemas = await _context.Cinemas.OrderBy(c => c.Name).ToListAsync();
            ViewData["CinemaId"] = new SelectList(cinemas, "CinemaId", "Name", room.CinemaId);
            return View(room);
        }

        // Hàm Helper để chuyển đổi dòng số (1, 2, 3) thành chữ cái (A, B, C... Z, AA, AB)
        private string GetRowLetter(int rowNumber)
        {
            int dividend = rowNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        // ── GET: /Rooms/SeatMap/{roomId} ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SeatMap(Guid id, bool editMode = false)
        {
            ViewBag.EditMode = editMode;
            var room = await _context.Rooms
                .Include(r => r.Cinema)
                .Include(r => r.Seats.OrderBy(s => s.RowLabel).ThenBy(s => s.ColNumber))
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room is null)
                return NotFound();

            if (room.Seats == null) room.Seats = new List<Seat>();

            // Auto-heal missing Seats (e.g. DB was seeded with corrupted data or missing seats)
            int expectedCount = room.TotalRows * room.TotalCols;
            if (room.Seats.Count < expectedCount)
            {
                var existingKeys = room.Seats.Select(s => $"{s.RowLabel}-{s.ColNumber}").ToHashSet();
                var missingSeats = new List<Seat>();

                for (int r = 1; r <= room.TotalRows; r++)
                {
                    string rowLetter = GetRowLetter(r);
                    for (int c = 1; c <= room.TotalCols; c++)
                    {
                        if (!existingKeys.Contains($"{r}-{c}"))
                        {
                            var newSeat = new Seat
                            {
                                SeatId = Guid.NewGuid(),
                                RoomId = room.RoomId,
                                RowLabel = r,
                                ColNumber = c,
                                SeatCode = $"{rowLetter}{c}",
                                SeatType = "Standard",
                                SeatStatusId = SeatStatusConstants.Inactive, // Những ghế lỗi không có trong DB thì mặc định khởi tạo mờ thành 'Lối đi' (Inactive)
                                CreatedAt = DateTime.UtcNow
                            };
                            missingSeats.Add(newSeat);
                        }
                    }
                }

                if (missingSeats.Any())
                {
                    await _context.Seats.AddRangeAsync(missingSeats);
                    await _context.SaveChangesAsync();
                    
                    // Add lại vào collection cho logic hiển thị Razor view ngay lập tức
                    foreach (var s in missingSeats) room.Seats.Add(s);
                }
            }

            return View(room);
        }

        // ── POST: /Rooms/UpdateSeats ──────────────────────────────────────────
        // Nhận danh sách ghế đã thay đổi từ JS client, cập nhật vào DB bằng Batch Update.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Authorize(Roles = "Admin")] // Bổ sung Authorization ở cấp Enterprise
        public async Task<IActionResult> UpdateSeats([FromBody] List<SeatUpdateRequest> seats)
        {
            if (seats == null || seats.Count == 0)
                return BadRequest(new { success = false, message = "Không có dữ liệu ghế nào được gửi lên." });

            // Mở Database Transaction để đảm bảo tính toàn vẹn (ACID)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ── BẮT ĐẦU: KIỂM TRA RÀNG BUỘC NGHIỆP VỤ (Sold Ticket Constraint) ──
                // Tìm những ghế bị chuyển từ Active sang trạng thái khác (Inactive/Maintenance)
                var nonActiveStatusIds = new[] { SeatStatusConstants.Inactive, SeatStatusConstants.Maintenance };
                var seatsToDisable = seats.Where(s => nonActiveStatusIds.Contains(s.SeatStatusId)).Select(s => s.Id).ToList();

                if (seatsToDisable.Any())
                {
                    // Business Rule: Không cho phép disable ghế nếu đã có vé bán (status != 0) ở các suất chiếu tương lai
                    var hasFutureBookings = await _context.ShowTimeSeats
                        .AnyAsync(sts => seatsToDisable.Contains(sts.SeatId) 
                                      && sts.Status != 0 // 0: Available, khác 0: Sold/Reserved
                                      && sts.ShowTime.StartAt > DateTime.UtcNow);

                    if (hasFutureBookings)
                    {
                        return BadRequest(new { success = false, message = "Không thể hủy kích hoạt ghế vì đã có khách đặt vé trong các suất chiếu tương lai." });
                    }
                }
                // ── KẾT THÚC: KIỂM TRA RÀNG BUỘC NGHIỆP VỤ ──

                int totalUpdated = 0;

                // 1. Group dữ liệu lại theo SeatType và SeatStatusId (Gom nhóm O(N))
                var groupedSeats = seats.GroupBy(s => new { s.SeatType, s.SeatStatusId }).ToList();

                foreach (var group in groupedSeats)
                {
                    // 2. Enum Validation: Đảm bảo Client không gửi lên các giá trị rác như SeatType = 99
                    if (!Enum.IsDefined(typeof(SeatTypeEnum), group.Key.SeatType))
                        continue;

                    var seatIdsToUpdate = group.Select(s => s.Id).ToList();
                    string seatTypeString = group.Key.SeatType.ToString();
                    Guid seatStatusId = group.Key.SeatStatusId;

                    // 3. Performance: Sử dụng ExecuteUpdateAsync của EF Core 7+ (Batch Update)
                    // - Bỏ qua hoàn toàn Change Tracker
                    // - Không tốn bước Load Data (SELECT) lên RAM
                    int updatedCount = await _context.Seats
                        .Where(s => seatIdsToUpdate.Contains(s.SeatId))
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.SeatType, seatTypeString)
                            .SetProperty(p => p.SeatStatusId, seatStatusId)
                            .SetProperty(p => p.LastUpdatedAt, DateTime.UtcNow)); // Cập nhật luôn Audit log
                    
                    totalUpdated += updatedCount;
                }

                await transaction.CommitAsync();

                if (totalUpdated == 0)
                    return NotFound(new { success = false, message = "Không tìm thấy dữ liệu ghế cần thay đổi trong cơ sở dữ liệu." });

                return Ok(new
                {
                    success = true,
                    message = $"Đã cập nhật {totalUpdated} ghế thành công.",
                    updated = totalUpdated
                });
            }
            // 4. Exception Handling chuyên biệt
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                // Bắt lỗi Database Operations riêng biệt. Ở production nên dùng ILogger để ghi log dbEx tại điểm này.
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống khi thiết lập hoặc ghi Cơ sở dữ liệu." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Bắt lỗi chung không mong muốn
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống không xác định: " + ex.Message });
            }
        }

        // GET: /Rooms/CheckRoomNameExists
        [HttpGet]
        public async Task<JsonResult> CheckRoomNameExists(Guid cinemaId, string roomName, Guid? excludeRoomId = null)
        {
            if (string.IsNullOrWhiteSpace(roomName))
                return Json(new { exists = false });

            bool exists = await _context.Rooms
                .AnyAsync(r => r.CinemaId == cinemaId 
                            && r.Name.Trim().ToLower() == roomName.Trim().ToLower()
                            && r.RoomId != excludeRoomId);

            return Json(new { exists });
        }
    }
}