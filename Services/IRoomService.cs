using CinemaManagement.Models;
using CinemaManagement.ViewModels.Rooms;

namespace CinemaManagement.Services
{
    public interface IRoomService
    {
        Task<RoomListViewModel> GetAllAsync(Guid? cinemaId, string? search, RoomStatus? status, string? sortBy, string? sortDir, int page = 1, int pageSize = 10);
        Task<(bool Success, string Message, int NewStatus)> ToggleStatusAsync(Guid id, int status);
        Task<Room?> GetByIdAsync(Guid id);
        Task<(bool Success, string Message, string? UpdatedName)> EditAsync(Guid id, string name, Guid? adminId = null);
        Task<(bool Success, string Message)> CreateAsync(Room room, Guid? adminId = null);
        Task<Room?> GetRoomWithSeatsAsync(Guid id, bool autoHeal = true);
        Task<(bool Success, string Message, int UpdatedCount)> UpdateSeatsAsync(List<SeatUpdateRequest> seats);
        Task<bool> IsRoomNameExistsAsync(Guid cinemaId, string name, Guid? excludeRoomId = null);
        Task<List<Cinema>> GetCinemasForDropdownAsync();
    }
}
