using CinemaManagement.ViewModels;

namespace CinemaManagement.Services
{
    public interface IShowTimeService
    {
        Task<ShowTimeListViewModel> GetShowTimeListAsync(
            string? search,
            DateTime? date,
            Guid? cinemaId,
            int? status,
            int? displayStatus,
            int page,
            int pageSize);

        Task<(bool IsOverlapping, List<object> Conflicts, bool IsPast)> CheckOverlapAsync(
            Guid roomId,
            DateTime startAt,
            Guid movieId,
            string? excludeId = null);

        Task CreateAsync(ShowTimeCreateViewModel model);

        Task EditAsync(Guid id, ShowTimeEditViewModel model);

        Task CancelAsync(Guid id);

        Task DeleteAsync(Guid id);

        Task<ShowTimeDetailViewModel?> GetDetailsAsync(Guid id);

        Task<ShowTimeEditViewModel?> GetEditViewModelAsync(Guid id);
    }
}
