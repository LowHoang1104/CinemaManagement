using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels.Rooms;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Services
{
    public class RoomService : IRoomService
    {
        private readonly CinemaManagementContext _context;

        public RoomService(CinemaManagementContext context)
        {
            _context = context;
        }

        public async Task<RoomListViewModel> GetAllAsync(Guid? cinemaId, string? search, RoomStatus? status, string? sortBy, string? sortDir, int page = 1, int pageSize = 10)
        {
            IQueryable<Room> baseQuery = _context.Rooms
                .Include(r => r.Cinema)
                .AsNoTracking();

            int totalRooms = await baseQuery.CountAsync();
            int totalActiveRooms = await baseQuery.CountAsync(r => r.Status == (int)RoomStatus.Active);
            int totalInactiveRooms = await baseQuery.CountAsync(r => r.Status == (int)RoomStatus.Inactive);
            int totalSeats = totalRooms == 0 ? 0 : await baseQuery.SumAsync(r => r.TotalRows * r.TotalCols);

            IQueryable<Room> roomsQuery = baseQuery;
            if (cinemaId.HasValue) roomsQuery = roomsQuery.Where(r => r.CinemaId == cinemaId.Value);
            if (status.HasValue) roomsQuery = roomsQuery.Where(r => r.Status == (int)status.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim().ToLower();
                roomsQuery = roomsQuery.Where(r => r.Name.ToLower().Contains(keyword) || (r.Cinema != null && r.Cinema.Name.ToLower().Contains(keyword)));
            }

            bool isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            roomsQuery = sortBy?.ToLower() switch
            {
                "name" => isDesc
                            ? roomsQuery.OrderByDescending(r => r.Name).ThenByDescending(r => r.RoomId)
                            : roomsQuery.OrderBy(r => r.Name).ThenBy(r => r.RoomId),
                "cinema" => isDesc
                            ? roomsQuery.OrderByDescending(r => r.Cinema!.Name).ThenByDescending(r => r.Name).ThenByDescending(r => r.RoomId)
                            : roomsQuery.OrderBy(r => r.Cinema!.Name).ThenBy(r => r.Name).ThenBy(r => r.RoomId),
                "seats" => isDesc
                            ? roomsQuery.OrderByDescending(r => r.TotalRows * r.TotalCols).ThenByDescending(r => r.RoomId)
                            : roomsQuery.OrderBy(r => r.TotalRows * r.TotalCols).ThenBy(r => r.RoomId),
                "date" => isDesc
                            ? roomsQuery.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.RoomId)
                            : roomsQuery.OrderBy(r => r.CreatedAt).ThenBy(r => r.RoomId),
                _ => roomsQuery.OrderBy(r => r.Cinema!.Name).ThenBy(r => r.Name).ThenBy(r => r.RoomId)
            };

            int totalItems = await roomsQuery.CountAsync();
            var rooms = await roomsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new RoomListViewModel
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
        }

        public async Task<(bool Success, string Message, int NewStatus)> ToggleStatusAsync(Guid id, int status)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return (false, "Không tìm thấy phòng chiếu.", 0);

            // Logic đã gỡ bỏ: Rạp không nhất thiết phải Active mới được kích hoạt phòng.
            // Điều này cho phép "xây dựng" phòng trước khi mở cửa rạp (Activate Cinema).
            else if (status == 0) // Ngừng hoạt động phòng
            {
                // Kiểm tra suất chiếu chưa kết thúc
                var hasActiveShowtimes = await _context.ShowTimes
                    .AnyAsync(s => s.RoomId == id && s.Status == 1 && s.EndAt >= DateTime.UtcNow);
                if (hasActiveShowtimes) 
                    return (false, "Không thể ngừng hoạt động! Phòng đang có lịch chiếu chưa kết thúc.", 0);

                // Kiểm tra vé đã bán trong tương lai (đảm bảo quyền lợi khách hàng)
                var hasFutureTickets = await _context.Tickets
                    .AnyAsync(t => t.ShowTime.RoomId == id && t.ShowTime.StartAt > DateTime.UtcNow);
                if (hasFutureTickets)
                    return (false, "Không thể ngừng hoạt động! Phòng vẫn còn vé đã bán cho các suất chiếu trong tương lai.", 0);
            }

            room.Status = status;
            try
            {
                _context.Update(room);
                await _context.SaveChangesAsync();
                string statusText = status == 1 ? "kích hoạt" : "ngừng hoạt động";
                return (true, $"Đã {statusText} phòng '{room.Name}' thành công.", status);
            }
            catch (Exception)
            {
                return (false, "Lỗi hệ thống khi lưu trạng thái.", 0);
            }
        }

        public async Task<Room?> GetByIdAsync(Guid id)
        {
            return await _context.Rooms.Include(r => r.Cinema).FirstOrDefaultAsync(m => m.RoomId == id);
        }

        public async Task<(bool Success, string Message, string? UpdatedName)> EditAsync(Guid id, string name, Guid? adminId = null)
        {
            var existingRoom = await _context.Rooms.FindAsync(id);
            if (existingRoom == null) return (false, "Không tìm thấy phòng chiếu.", null);

            bool isRoomNameExists = await _context.Rooms
                .AnyAsync(r => r.CinemaId == existingRoom.CinemaId && r.RoomId != id && r.Name.Trim().ToLower() == name.Trim().ToLower());
            if (isRoomNameExists) return (false, "Tên phòng chiếu này đã tồn tại trong rạp.", null);

            existingRoom.Name = name;
            existingRoom.LastUpdatedAt = DateTime.UtcNow;
            if (adminId.HasValue) existingRoom.LastUpdatedBy = adminId;

            _context.Update(existingRoom);
            await _context.SaveChangesAsync();
            return (true, $"Đã cập nhật tên phòng thành '{existingRoom.Name}'.", existingRoom.Name);
        }

        public async Task<(bool Success, string Message)> CreateAsync(Room room, Guid? adminId = null)
        {
            var cinema = await _context.Cinemas.FindAsync(room.CinemaId);
            if (cinema == null) return (false, "Không tìm thấy rạp chiếu được chọn.");
            // Logic đã gỡ bỏ: Cho phép tạo phòng mới ngay cả khi rạp đang ngừng hoạt động (đang chuẩn bị mở rạp).

            bool isRoomNameExists = await _context.Rooms
                .AnyAsync(r => r.CinemaId == room.CinemaId && r.Name.Trim().ToLower() == room.Name.Trim().ToLower());
            
            if (isRoomNameExists) return (false, "Tên phòng chiếu này đã tồn tại trong rạp được chọn.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                room.RoomId = Guid.NewGuid();
                room.Status = (int)RoomStatus.Active;
                room.CreatedAt = DateTime.UtcNow;
                room.CreatedBy = adminId;

                _context.Rooms.Add(room);

                var seatsToInsert = new List<Seat>();
                for (int r = 1; r <= room.TotalRows; r++)
                {
                    string rowLetter = GetRowLetter(r);
                    for (int c = 1; c <= room.TotalCols; c++)
                    {
                        seatsToInsert.Add(new Seat
                        {
                            SeatId = Guid.NewGuid(),
                            RoomId = room.RoomId,
                            SeatCode = $"{rowLetter}{c}",
                            RowLabel = r,
                            ColNumber = c,
                            SeatType = SeatTypeEnum.Standard.ToString(),
                            SeatStatusId = SeatStatusConstants.Active,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = adminId
                        });
                    }
                }

                await _context.Seats.AddRangeAsync(seatsToInsert);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Đã tạo phòng '{room.Name}' và sinh tự động {room.TotalRows * room.TotalCols} ghế thành công!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Đã xảy ra lỗi hệ thống khi tạo phòng và ghế. Chi tiết: " + ex.Message);
            }
        }

        public async Task<Room?> GetRoomWithSeatsAsync(Guid id, bool autoHeal = true)
        {
            var room = await _context.Rooms
                .Include(r => r.Cinema)
                .Include(r => r.Seats.OrderBy(s => s.RowLabel).ThenBy(s => s.ColNumber))
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null) return null;
            if (room.Seats == null) room.Seats = new List<Seat>();

            if (autoHeal && room.Seats.Count < (room.TotalRows * room.TotalCols))
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
                            missingSeats.Add(new Seat
                            {
                                SeatId = Guid.NewGuid(),
                                RoomId = room.RoomId,
                                RowLabel = r,
                                ColNumber = c,
                                SeatCode = $"{rowLetter}{c}",
                                SeatType = "Standard",
                                SeatStatusId = SeatStatusConstants.Inactive,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                if (missingSeats.Any())
                {
                    await _context.Seats.AddRangeAsync(missingSeats);
                    await _context.SaveChangesAsync();
                    foreach (var s in missingSeats) room.Seats.Add(s);
                }
            }

            return room;
        }

        public async Task<(bool Success, string Message, int UpdatedCount)> UpdateSeatsAsync(List<SeatUpdateRequest> seats)
        {
            if (seats == null || seats.Count == 0) return (false, "Không có dữ liệu ghế nào được gửi lên.", 0);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nonActiveStatusIds = new[] { SeatStatusConstants.Inactive, SeatStatusConstants.Maintenance };
                var seatsToDisable = seats.Where(s => nonActiveStatusIds.Contains(s.SeatStatusId)).Select(s => s.Id).ToList();

                if (seatsToDisable.Any())
                {
                    var hasFutureBookings = await _context.ShowTimeSeats
                        .AnyAsync(sts => seatsToDisable.Contains(sts.SeatId) && sts.Status != 0 && sts.ShowTime.StartAt > DateTime.UtcNow);
                    if (hasFutureBookings) return (false, "Không thể hủy kích hoạt ghế vì đã có khách đặt vé trong suất chiếu tương lai.", 0);
                }

                int totalUpdated = 0;
                var groupedSeats = seats.GroupBy(s => new { s.SeatType, s.SeatStatusId }).ToList();

                foreach (var group in groupedSeats)
                {
                    if (!Enum.IsDefined(typeof(SeatTypeEnum), group.Key.SeatType)) continue;

                    var seatIdsToUpdate = group.Select(s => s.Id).ToList();
                    string seatTypeString = group.Key.SeatType.ToString();
                    Guid seatStatusId = group.Key.SeatStatusId;

                    int updatedCount = await _context.Seats
                        .Where(s => seatIdsToUpdate.Contains(s.SeatId))
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.SeatType, seatTypeString)
                            .SetProperty(p => p.SeatStatusId, seatStatusId)
                            .SetProperty(p => p.LastUpdatedAt, DateTime.UtcNow));

                    totalUpdated += updatedCount;
                }

                await transaction.CommitAsync();
                
                if (totalUpdated == 0) return (false, "Không tìm thấy dữ liệu ghế cần thay đổi.", 0);
                return (true, $"Đã cập nhật {totalUpdated} ghế thành công.", totalUpdated);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Lỗi hệ thống: " + ex.Message, 0);
            }
        }

        public async Task<bool> IsRoomNameExistsAsync(Guid cinemaId, string name, Guid? excludeRoomId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return await _context.Rooms.AnyAsync(r => r.CinemaId == cinemaId && r.Name.Trim().ToLower() == name.Trim().ToLower() && r.RoomId != excludeRoomId);
        }

        public async Task<List<Cinema>> GetCinemasForDropdownAsync()
        {
            return await _context.Cinemas.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        }

        private string GetRowLetter(int rowNumber)
        {
            int dividend = rowNumber;
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
}
