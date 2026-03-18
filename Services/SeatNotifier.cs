using CinemaManagement.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CinemaManagement.Services;

/// <summary>
/// Service để gửi thông báo cập nhật trạng thái ghế qua SignalR
/// </summary>
public class SeatNotifier : ISeatNotifier
{
    private readonly IHubContext<SeatHub> _hubContext;
    private readonly ILogger<SeatNotifier> _logger;

    public SeatNotifier(IHubContext<SeatHub> hubContext, ILogger<SeatNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Thông báo ghế đã được đặt cho tất cả client
    /// </summary>
    public async Task NotifySeatBooked(string showTimeId, string seatId, string seatCode)
    {
        try
        {
            _logger.LogInformation("[SeatNotifier] Broadcasting SeatBooked: showTimeId={showTimeId}, seatId={seatId}, seatCode={seatCode}", 
                showTimeId, seatId, seatCode);

            await _hubContext.Clients.Group(showTimeId).SendAsync("SeatBooked", new
            {
                showTimeId,
                seatId,
                seatCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SeatNotifier] Error broadcasting SeatBooked");
        }
    }

    /// <summary>
    /// Thông báo ghế được release (trở lại trạng thái có sẵn) cho tất cả client
    /// </summary>
    public async Task NotifySeatsReleased(string showTimeId, List<string> seatIds)
    {
        try
        {
            _logger.LogInformation("[SeatNotifier] Broadcasting SeatsReleased: showTimeId={showTimeId}, seatCount={seatCount}", 
                showTimeId, seatIds.Count);

            await _hubContext.Clients.Group(showTimeId).SendAsync("SeatsReleased", new
            {
                showTimeId,
                seatIds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SeatNotifier] Error broadcasting SeatsReleased");
        }
    }
}
