namespace CinemaManagement.Services;

/// <summary>
/// Interface để gửi thông báo cập nhật trạng thái ghế qua SignalR
/// </summary>
public interface ISeatNotifier
{
    Task NotifySeatBooked(string showTimeId, string seatId, string seatCode);
    Task NotifySeatsReleased(string showTimeId, List<string> seatIds);
}
