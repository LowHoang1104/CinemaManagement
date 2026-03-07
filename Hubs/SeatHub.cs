using Microsoft.AspNetCore.SignalR;

namespace CinemaManagement.Hubs;

/// <summary>
/// SignalR Hub: broadcast trạng thái ghế real-time.
/// Client join group theo ShowTimeId để nhận updates.
/// </summary>
public class SeatHub : Hub
{
    // Client gọi để join group của một suất chiếu
    public async Task JoinShowTime(string showTimeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, showTimeId);
    }

    public async Task LeaveShowTime(string showTimeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, showTimeId);
    }
}

/// <summary>
/// Service inject vào các controllers/services để broadcast seat events.
/// </summary>
public interface ISeatNotifier
{
    Task NotifySeatHeld(string showTimeId, string seatId, string seatCode, string sessionId);
    Task NotifySeatReleased(string showTimeId, string seatId, string seatCode);
    Task NotifySeatBooked(string showTimeId, string seatId, string seatCode);
}

public class SeatNotifier : ISeatNotifier
{
    private readonly IHubContext<SeatHub> _hub;

    public SeatNotifier(IHubContext<SeatHub> hub) => _hub = hub;

    public Task NotifySeatHeld(string showTimeId, string seatId, string seatCode, string sessionId)
        => _hub.Clients.Group(showTimeId)
            .SendAsync("SeatStatusChanged", new
            {
                seatId,
                seatCode,
                status = "Holding",
                sessionId
            });

    public Task NotifySeatReleased(string showTimeId, string seatId, string seatCode)
        => _hub.Clients.Group(showTimeId)
            .SendAsync("SeatStatusChanged", new
            {
                seatId,
                seatCode,
                status = "Available"
            });

    public Task NotifySeatBooked(string showTimeId, string seatId, string seatCode)
        => _hub.Clients.Group(showTimeId)
            .SendAsync("SeatStatusChanged", new
            {
                seatId,
                seatCode,
                status = "Booked"
            });
}