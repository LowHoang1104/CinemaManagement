using CinemaManagement.ViewModels.AdminMovies;

namespace CinemaManagement.Services;

public interface IAdminMovieTcpService
{
    Task<AdminMovieServiceResult<IReadOnlyList<AdminMovieViewModel>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AdminMovieServiceResult<AdminMovieViewModel>> GetByIdAsync(Guid movieId, CancellationToken cancellationToken = default);

    Task<AdminMovieServiceResult<AdminMovieViewModel>> CreateAsync(AdminMovieViewModel model, CancellationToken cancellationToken = default);

    Task<AdminMovieServiceResult<AdminMovieViewModel>> UpdateAsync(Guid movieId, AdminMovieViewModel model, CancellationToken cancellationToken = default);

    Task<AdminMovieServiceResult<bool>> DeleteAsync(Guid movieId, CancellationToken cancellationToken = default);
}

public sealed class AdminMovieServiceResult<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public static AdminMovieServiceResult<T> Ok(string message, T? data = default)
        => new() { Success = true, Message = message, Data = data };

    public static AdminMovieServiceResult<T> Fail(string message, T? data = default)
        => new() { Success = false, Message = message, Data = data };
}
