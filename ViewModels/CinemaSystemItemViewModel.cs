namespace CinemaManagement.ViewModels;

public class CinemaSystemItemViewModel
{
    public string CinemaName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int ActiveRooms { get; set; }
    public int TotalSeats { get; set; }
}