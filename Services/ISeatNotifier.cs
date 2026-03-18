namespace CinemaManagement.Services;

/// <summary>
/// Interface ?? g?i thông báo c?p nh?t tr?ng thái gh? qua SignalR
/// </summary>
public interface ISeatNotifier
{
    Task NotifySeatBooked(string showTimeId, string seatId, string seatCode);
    Task NotifySeatsReleased(string showTimeId, List<string> seatIds);
}
