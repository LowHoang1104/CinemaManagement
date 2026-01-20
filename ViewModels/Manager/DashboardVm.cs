namespace CinemaManagement.ViewModels.Manager;
public class DashboardVm
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int TotalTicketsSold { get; set; }
    public int TotalCancelled { get; set; }

    public List<RevenueByDayVm> RevenueByDay { get; set; } = new();
    public List<TopMovieVm> TopMovies { get; set; } = new();
}

public class RevenueByDayVm
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
}

public class TopMovieVm
{
    public Guid MovieId { get; set; }
    public string Title { get; set; } = "";
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
}
