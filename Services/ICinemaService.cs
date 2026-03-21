using CinemaManagement.Models;
using CinemaManagement.Requests;
using CinemaManagement.ViewModels.Cinema;

namespace CinemaManagement.Services
{
    public interface ICinemaService
    {
        // Task<List<Cinema>> GetAllAsync(string? search = null, int? status = null, string? sortBy = null, string? sortDir = null);
        Task<(List<Cinema> Items, int TotalItems)> GetAllAsync(string? search = null, int? status = null, string? sortBy = null, string? sortDir = null, int page = 1, int pageSize = 2);
        Task<CinemaStatsVm> GetStatsAsync();
        Task<CinemaDetailsVm?> GetCinemaDetailsAsync(Guid id);
        Task<Cinema> GetByIdAsync(Guid id);
        Task CreateAsync(CreateCinemaRequest request, Guid? userId);
        Task UpdateAsync(UpdateCinemaRequest request, Guid? userId);

        Task ActivateAsync(Guid id, Guid? userId);
        Task DeactivateAsync(Guid id, Guid? userId);
        Task<EditCinemaVm?> GetEditByIdAsync(Guid id);
        Task<bool> IsCinemaNameExistsAsync(string name, Guid? excludeCinemaId = null);
    }

}
