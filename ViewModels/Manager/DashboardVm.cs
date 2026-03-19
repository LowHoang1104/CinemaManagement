namespace CinemaManagement.ViewModels.Manager;

public class DashboardVm
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    // KPIs
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int TotalTicketsSold { get; set; }
    public int TotalCancelled { get; set; }

    // Computed
    public double CancellationRate =>
        TotalBookings > 0 ? (double)TotalCancelled / TotalBookings * 100 : 0;

    // Charts
    public List<RevenueByDayVm> RevenueByDay { get; set; } = new();
    public List<TopMovieVm> TopMovies { get; set; } = new();

    // Occupancy by showtime (top 10)
    public List<ShowtimeOccupancyVm> OccupancyRates { get; set; } = new();
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

public class ShowtimeOccupancyVm
{
    public Guid ShowTimeId { get; set; }
    public string MovieTitle { get; set; } = "";
    public DateTime StartAt { get; set; }
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; }
    public double OccupancyRate => TotalSeats > 0
        ? (double)BookedSeats / TotalSeats * 100 : 0;
}