using CinemaManagement.Data;
using CinemaManagement.ViewModels.Manager;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Services;

public interface IReportService
{
    Task<DashboardVm> GetDashboardAsync(DateTime from, DateTime to);
}
public class ReportService : IReportService
{
    private readonly CinemaManagementContext _db;
    public ReportService(CinemaManagementContext db) => _db = db;

    public async Task<DashboardVm> GetDashboardAsync(DateTime from, DateTime to)
    {
        // normalize date range (UTC or local tuỳ bạn)
        var fromDate = from.Date;
        var toDateExclusive = to.Date.AddDays(1);

        // giả định: Payment.Status = 1 là Success (bạn chỉnh theo enum thật)
        const short PAYMENT_SUCCESS = 1;

        // Revenue = sum payments success
        var totalRevenue = await _db.Payments.AsNoTracking()
            .Where(p => p.PaidAt != null
                        && p.PaidAt >= fromDate && p.PaidAt < toDateExclusive
                        && p.Status == PAYMENT_SUCCESS)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        // bookings
        var totalBookings = await _db.Bookings.AsNoTracking()
            .Where(b => b.CreatedAt >= fromDate && b.CreatedAt < toDateExclusive)
            .CountAsync();

        // cancelled/expired (giả định Booking.Status: 0 Pending, 1 Paid, 2 Cancelled, 3 Expired)
        const short BOOKING_CANCELLED = 2;
        const short BOOKING_EXPIRED = 3;

        var totalCancelled = await _db.Bookings.AsNoTracking()
            .Where(b => b.CreatedAt >= fromDate && b.CreatedAt < toDateExclusive
                        && (b.Status == BOOKING_CANCELLED || b.Status == BOOKING_EXPIRED))
            .CountAsync();

        // tickets sold = tickets of paid bookings
        const short BOOKING_PAID = 1;

        var totalTicketsSold = await _db.Tickets.AsNoTracking()
            .Where(t => t.Booking.CreatedAt >= fromDate && t.Booking.CreatedAt < toDateExclusive
                        && t.Booking.Status == BOOKING_PAID)
            .CountAsync();

        // Revenue by day
        var revenueByDay = await _db.Payments.AsNoTracking()
            .Where(p => p.PaidAt != null
                        && p.PaidAt >= fromDate && p.PaidAt < toDateExclusive
                        && p.Status == PAYMENT_SUCCESS)
            .GroupBy(p => p.PaidAt!.Value.Date)
            .Select(g => new RevenueByDayVm
            {
                Date = g.Key,
                Revenue = g.Sum(x => x.Amount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        // Top movies by revenue (paid bookings)
        var topMovies = await _db.Tickets.AsNoTracking()
            .Where(t => t.Booking.Status == BOOKING_PAID
                        && t.Booking.CreatedAt >= fromDate && t.Booking.CreatedAt < toDateExclusive)
            .GroupBy(t => new { t.ShowTime.MovieId, t.ShowTime.Movie.Title })
            .Select(g => new TopMovieVm
            {
                MovieId = g.Key.MovieId,
                Title = g.Key.Title,
                TicketsSold = g.Count(),
                Revenue = g.Sum(x => x.UnitPrice)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync();

        return new DashboardVm
        {
            From = fromDate,
            To = to.Date,
            TotalRevenue = totalRevenue,
            TotalBookings = totalBookings,
            TotalTicketsSold = totalTicketsSold,
            TotalCancelled = totalCancelled,
            RevenueByDay = revenueByDay,
            TopMovies = topMovies
        };
    }
}

