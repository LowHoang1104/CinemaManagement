using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.Requests;
using CinemaManagement.ViewModels.Cinema;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Services
{
    public class CinemaService : ICinemaService
    {
        private readonly CinemaManagementContext _context;

        public CinemaService(CinemaManagementContext context)
        {
            _context = context;
        }

        public async Task<(List<Cinema> Items, int TotalItems)> GetAllAsync(string? search = null, int? status = null, string? sortBy = null, string? sortDir = null, int page = 1, int pageSize = 2)
        {
            var query = _context.Cinemas.Include(c => c.Rooms).AsNoTracking(); // Thêm AsNoTracking để tối ưu tốc độ đọc

            // 1. Lọc (Filtering)
            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(s) || c.Address.ToLower().Contains(s));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            // Đếm tổng số lượng bản ghi thỏa mãn bộ lọc (để làm phân trang)
            int totalItems = await query.CountAsync();

            // 2. Sắp xếp (Sorting) + Stable Sorting (Chống nhảy trang)
            query = sortBy?.ToLower() switch
            {
                "name" => sortDir == "desc"
                          ? query.OrderByDescending(c => c.Name).ThenByDescending(c => c.CinemaId)
                          : query.OrderBy(c => c.Name).ThenBy(c => c.CinemaId),

                "rooms" => sortDir == "desc"
                           ? query.OrderByDescending(c => c.Rooms.Count).ThenByDescending(c => c.CinemaId)
                           : query.OrderBy(c => c.Rooms.Count).ThenBy(c => c.CinemaId),

                _ => query.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.CinemaId)
            };

            // 3. Phân trang (Pagination)
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalItems);
        }

        public async Task<CinemaStatsViewModel> GetStatsAsync()
        {
            // Tối ưu hóa: Để SQL Server tự đếm, không kéo dữ liệu về RAM
            var baseQuery = _context.Cinemas.AsNoTracking();

            return new CinemaStatsViewModel
            {
                TotalCinemas = await baseQuery.CountAsync(),
                ActiveCinemas = await baseQuery.CountAsync(c => c.Status == 1),
                InactiveCinemas = await baseQuery.CountAsync(c => c.Status == 0),
                TotalRooms = await _context.Rooms.CountAsync() // Đếm thẳng từ bảng Rooms sẽ nhanh hơn rất nhiều
            };
        }

        public async Task<Cinema> GetByIdAsync(Guid id)
        {
            return await _context.Cinemas.FindAsync(id)
               ?? throw new Exception("Cinema not found");
        }

        public async Task CreateAsync(CreateCinemaRequest request, Guid? userId)
        {
            // 1. Chống trùng lặp (Kiểm tra không phân biệt hoa thường và khoảng trắng)
            bool isDuplicate = await _context.Cinemas
                .AnyAsync(c => c.Name.Trim().ToLower() == request.Name.Trim().ToLower());

            if (isDuplicate)
            {
                // Ném lỗi Business Logic để Controller bắt
                throw new InvalidOperationException($"Rạp chiếu phim mang tên '{request.Name}' đã tồn tại trong hệ thống.");
            }

            var cinema = new Cinema
            {
                CinemaId = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Address = request.Address.Trim(),
                Status = 0, // Mặc định: Ngừng hoạt động (Inactive)
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId // Dấu vết kiểm toán (Audit Trail)
            };

            _context.Cinemas.Add(cinema);
            await _context.SaveChangesAsync();
        }

        // Thêm tham số excludeCinemaId
        public async Task<bool> IsCinemaNameExistsAsync(string name, Guid? excludeCinemaId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            string trimmedName = name.Trim().ToLower();

            var query = _context.Cinemas.Where(c => c.Name.ToLower() == trimmedName);

            // Nếu đang Edit, bỏ qua ID của chính nó
            if (excludeCinemaId.HasValue)
            {
                query = query.Where(c => c.CinemaId != excludeCinemaId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task UpdateAsync(UpdateCinemaRequest request, Guid? userId)
        {
            // Check trùng tên (loại trừ chính nó) trước khi lưu
            bool isDuplicate = await IsCinemaNameExistsAsync(request.Name, request.CinemaId);
            if (isDuplicate) throw new InvalidOperationException($"Rạp chiếu phim mang tên '{request.Name}' đã tồn tại.");

            var cinema = await _context.Cinemas.FindAsync(request.CinemaId)
                         ?? throw new Exception("Cinema not found");

            cinema.Name = request.Name;
            cinema.Address = request.Address;
            cinema.LastUpdatedAt = DateTime.UtcNow;
            cinema.LastUpdatedBy = userId;

            await _context.SaveChangesAsync();
        }

        public async Task ActivateAsync(Guid id, Guid? userId)
        {
            var cinema = await _context.Cinemas.Include(c => c.Rooms)
                .FirstOrDefaultAsync(c => c.CinemaId == id) ?? throw new Exception("Cinema not found");

            if (cinema.Status == 1) throw new Exception("Cinema already active");

            // Logic mới: Rạp phải có ít nhất 1 phòng đang hoạt động (Status = 1)
            if (!cinema.Rooms.Any(r => r.Status == 1)) 
                throw new Exception("Rạp phải có ít nhất một phòng đang hoạt động mới có thể mở cửa.");

            cinema.Status = 1;
            cinema.LastUpdatedAt = DateTime.UtcNow;
            cinema.LastUpdatedBy = userId;

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(Guid id, Guid? userId)
        {
            var cinema = await _context.Cinemas.Include(c => c.Rooms)
                .FirstOrDefaultAsync(c => c.CinemaId == id) ?? throw new Exception("Cinema not found");

            if (cinema.Status == 0)
                throw new Exception("Cinema already inactive");

            // Logic mới: Kiểm tra nếu còn vé đã bán cho các suất chiếu trong tương lai
            bool hasFutureTickets = await _context.Tickets
                .AnyAsync(t => t.ShowTime.Room.CinemaId == id && t.ShowTime.StartAt > DateTime.UtcNow);

            if (hasFutureTickets)
                throw new Exception("Không thể đóng rạp vì vẫn còn vé đã bán cho các suất chiếu tương lai. Vui lòng xử lý hoàn tiền hoặc hủy suất chiếu trước khi đóng rạp.");

            cinema.Status = 0;
            cinema.LastUpdatedAt = DateTime.UtcNow;
            cinema.LastUpdatedBy = userId;

            await _context.SaveChangesAsync();
        }

        public async Task<CinemaDetailsViewModel?> GetCinemaDetailsAsync(Guid id)
        {
            return await _context.Cinemas
                .Include(c => c.Rooms)
                .Where(c => c.CinemaId == id)
                .Select(c => new CinemaDetailsViewModel
                {
                    CinemaId = c.CinemaId,
                    Name = c.Name,
                    Address = c.Address,
                    Status = c.Status,
                    Rooms = c.Rooms.Select(r => new RoomVm
                    {
                        RoomId = r.RoomId,
                        Name = r.Name,
                        Capacity = r.TotalRows * r.TotalCols,
                        Status = r.Status
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<EditCinemaViewModel?> GetEditByIdAsync(Guid id)
        {
            return await _context.Cinemas
                .Where(c => c.CinemaId == id)
                .Select(c => new EditCinemaViewModel
                {
                    CinemaId = c.CinemaId,
                    Name = c.Name,
                    Address = c.Address
                })
                .FirstOrDefaultAsync();
        }
    }
}
