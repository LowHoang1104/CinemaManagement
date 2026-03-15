using CinemaManagement.ViewModels.AdminMovies;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace CinemaManagement.Services;

public sealed class AdminMovieTcpService : IAdminMovieTcpService
{
    private readonly TcpMovieAdminOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public AdminMovieTcpService(IOptions<TcpMovieAdminOptions> options)
    {
        _options = options.Value;
    }

    public async Task<AdminMovieServiceResult<IReadOnlyList<AdminMovieViewModel>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(new TcpMovieRequest
        {
            Command = "LIST"
        }, cancellationToken);

        if (!response.Success)
        {
            return AdminMovieServiceResult<IReadOnlyList<AdminMovieViewModel>>.Fail(response.Message, Array.Empty<AdminMovieViewModel>());
        }

        var data = response.ReadData<List<AdminMovieViewModel>>(_jsonOptions) ?? [];
        return AdminMovieServiceResult<IReadOnlyList<AdminMovieViewModel>>.Ok(response.Message, data);
    }

    public async Task<AdminMovieServiceResult<AdminMovieViewModel>> GetByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(new TcpMovieRequest
        {
            Command = "GET",
            MovieId = movieId
        }, cancellationToken);

        if (!response.Success)
        {
            return AdminMovieServiceResult<AdminMovieViewModel>.Fail(response.Message);
        }

        var data = response.ReadData<AdminMovieViewModel>(_jsonOptions);
        return data is null
            ? AdminMovieServiceResult<AdminMovieViewModel>.Fail("Không đọc được dữ liệu phim từ TCP service.")
            : AdminMovieServiceResult<AdminMovieViewModel>.Ok(response.Message, data);
    }

    public async Task<AdminMovieServiceResult<AdminMovieViewModel>> CreateAsync(AdminMovieViewModel model, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(new TcpMovieRequest
        {
            Command = "CREATE",
            Movie = MapPayload(model)
        }, cancellationToken);

        if (!response.Success)
        {
            return AdminMovieServiceResult<AdminMovieViewModel>.Fail(response.Message);
        }

        var data = response.ReadData<AdminMovieViewModel>(_jsonOptions);
        return AdminMovieServiceResult<AdminMovieViewModel>.Ok(response.Message, data);
    }

    public async Task<AdminMovieServiceResult<AdminMovieViewModel>> UpdateAsync(Guid movieId, AdminMovieViewModel model, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(new TcpMovieRequest
        {
            Command = "UPDATE",
            MovieId = movieId,
            Movie = MapPayload(model)
        }, cancellationToken);

        if (!response.Success)
        {
            return AdminMovieServiceResult<AdminMovieViewModel>.Fail(response.Message);
        }

        var data = response.ReadData<AdminMovieViewModel>(_jsonOptions);
        return AdminMovieServiceResult<AdminMovieViewModel>.Ok(response.Message, data);
    }

    public async Task<AdminMovieServiceResult<bool>> DeleteAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(new TcpMovieRequest
        {
            Command = "DELETE",
            MovieId = movieId
        }, cancellationToken);

        return response.Success
            ? AdminMovieServiceResult<bool>.Ok(response.Message, true)
            : AdminMovieServiceResult<bool>.Fail(response.Message, false);
    }

    private async Task<TcpMovieResponse> SendAsync(TcpMovieRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_options.Host, _options.Port, cancellationToken);

            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true)
            {
                AutoFlush = true
            };

            var requestPayload = JsonSerializer.Serialize(request, _jsonOptions);
            await writer.WriteLineAsync(requestPayload);

            var responseLine = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(responseLine))
            {
                return TcpMovieResponse.Fail("TCP service không trả dữ liệu.");
            }

            var response = JsonSerializer.Deserialize<TcpMovieResponse>(responseLine, _jsonOptions);
            return response ?? TcpMovieResponse.Fail("Không parse được phản hồi từ TCP service.");
        }
        catch (SocketException)
        {
            return TcpMovieResponse.Fail($"Không kết nối được TCP service {_options.Host}:{_options.Port}. Hãy chạy ServiceMovieAdmin trước.");
        }
        catch (OperationCanceledException)
        {
            return TcpMovieResponse.Fail("Yêu cầu TCP đã bị hủy.");
        }
        catch (Exception ex)
        {
            return TcpMovieResponse.Fail($"TCP error: {ex.Message}");
        }
    }

    private static TcpMoviePayload MapPayload(AdminMovieViewModel model) => new()
    {
        Title = model.Title,
        DurationMin = model.DurationMin,
        Description = model.Description,
        PosterUrl = model.PosterUrl,
        AgeRating = model.AgeRating,
        Director = model.Director,
        Actors = model.Actors,
        Genre = model.Genre,
        Language = model.Language,
        ReleaseDate = model.ReleaseDate,
        Status = model.Status
    };

    private sealed class TcpMovieRequest
    {
        public string Command { get; set; } = string.Empty;

        public Guid? MovieId { get; set; }

        public TcpMoviePayload? Movie { get; set; }
    }

    private sealed class TcpMoviePayload
    {
        public string Title { get; set; } = string.Empty;

        public int DurationMin { get; set; }

        public string? Description { get; set; }

        public string? PosterUrl { get; set; }

        public int? AgeRating { get; set; }

        public string? Director { get; set; }

        public string? Actors { get; set; }

        public string? Genre { get; set; }

        public string? Language { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public int Status { get; set; }
    }

    private sealed class TcpMovieResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public JsonElement? Data { get; set; }

        public static TcpMovieResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };

        public T? ReadData<T>(JsonSerializerOptions options)
        {
            if (!Data.HasValue)
            {
                return default;
            }

            var jsonData = Data.Value;
            if (jsonData.ValueKind == JsonValueKind.Null || jsonData.ValueKind == JsonValueKind.Undefined)
            {
                return default;
            }

            return jsonData.Deserialize<T>(options);
        }
    }
}
