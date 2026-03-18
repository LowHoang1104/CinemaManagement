namespace CinemaManagement.ViewModels;

public class TicketPriceItemViewModel
{
    public string MovieTitle { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public decimal BasePrice { get; set; }
}