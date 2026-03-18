using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Services;

/// <summary>
/// Service xử lý logic couple seat
/// </summary>
public interface ICoupleSeatService
{
    /// <summary>
    /// Lấy ghế đôi (cặp ghế)
    /// </summary>
    Task<List<Seat>> GetCoupleSeatsAsync(Guid showTimeId, Seat seat);

    /// <summary>
    /// Check xem ghế có phải couple seat không
    /// </summary>
    bool IsCoupleSeat(Seat seat);

    /// <summary>
    /// Lấy ColNumber của ghế đôi
    /// </summary>
    int GetPairedColNumber(Seat seat);

    /// <summary>
    /// Hold couple seats (nếu là couple) hoặc single seat
    /// </summary>
    Task<List<Seat>> HoldSeatsAsync(Guid showTimeId, Seat seat, string holdSessionId, DateTime holdUntil);

    /// <summary>
    /// Release couple seats (nếu là couple) hoặc single seat
    /// </summary>
    Task<List<Seat>> ReleaseSeatsAsync(Guid showTimeId, Seat seat);

    /// <summary>
    /// Book couple seats (nếu là couple) hoặc single seat
    /// </summary>
    Task<List<Seat>> BookSeatsAsync(Guid showTimeId, Seat seat);

    /// <summary>
    /// Kiểm tra xem couple seats có available không
    /// </summary>
    Task<bool> AreCoupleSeatsAvailableAsync(Guid showTimeId, Seat seat);
}

public class CoupleSeatService : ICoupleSeatService
{
    private readonly CinemaManagementContext _context;
    private readonly ILogger<CoupleSeatService> _logger;

    public CoupleSeatService(CinemaManagementContext context, ILogger<CoupleSeatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public bool IsCoupleSeat(Seat seat)
    {
        return seat?.SeatType == "Couple";
    }

    public int GetPairedColNumber(Seat seat)
    {
        if (!IsCoupleSeat(seat))
            return seat.ColNumber;

        // Even -> pair with ColNumber - 1
        // Odd -> pair with ColNumber + 1
        return seat.ColNumber % 2 == 0 ? seat.ColNumber - 1 : seat.ColNumber + 1;
    }

    public async Task<List<Seat>> GetCoupleSeatsAsync(Guid showTimeId, Seat seat)
    {
        var result = new List<Seat> { seat };

        if (!IsCoupleSeat(seat))
            return result;

        try
        {
            var pairedColNumber = GetPairedColNumber(seat);

            // Find paired seat in same row and room
            var pairedSeat = await _context.Seats
                .FirstOrDefaultAsync(s =>
                    s.RoomId == seat.RoomId &&
                    s.RowLabel == seat.RowLabel &&
                    s.ColNumber == pairedColNumber);

            if (pairedSeat != null)
            {
                result.Add(pairedSeat);
                _logger.LogInformation("[CoupleSeat] Found paired seat: {seatCode1} <-> {seatCode2}",
                    seat.SeatCode, pairedSeat.SeatCode);
            }
            else
            {
                _logger.LogWarning("[CoupleSeat] Paired seat not found for {seatCode} (looking for col {pairedCol})",
                    seat.SeatCode, pairedColNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CoupleSeat] Error getting paired seat");
        }

        return result;
    }

    public async Task<bool> AreCoupleSeatsAvailableAsync(Guid showTimeId, Seat seat)
    {
        var coupleSeats = await GetCoupleSeatsAsync(showTimeId, seat);

        // Check if all seats in the couple are available (Status = 0)
        var seatIds = coupleSeats.Select(s => s.SeatId).ToList();
        var showTimeSeats = await _context.ShowTimeSeats
            .Where(sts => sts.ShowTimeId == showTimeId && seatIds.Contains(sts.SeatId))
            .ToListAsync();

        // All seats must be found and available
        if (showTimeSeats.Count != coupleSeats.Count)
        {
            _logger.LogWarning("[CoupleSeat] Mismatch: expected {count} ShowTimeSeats, found {found}",
                coupleSeats.Count, showTimeSeats.Count);
            return false;
        }

        var allAvailable = showTimeSeats.All(sts => sts.Status == 0);
        _logger.LogInformation("[CoupleSeat] Availability check for {seatCode}: {available}",
            seat.SeatCode, allAvailable);

        return allAvailable;
    }

    public async Task<List<Seat>> HoldSeatsAsync(Guid showTimeId, Seat seat, string holdSessionId, DateTime holdUntil)
    {
        var coupleSeats = await GetCoupleSeatsAsync(showTimeId, seat);
        var seatIds = coupleSeats.Select(s => s.SeatId).ToList();

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var showTimeSeats = await _context.ShowTimeSeats
                .Where(sts => sts.ShowTimeId == showTimeId && seatIds.Contains(sts.SeatId))
                .ToListAsync();

            foreach (var sts in showTimeSeats)
            {
                sts.Status = 1; // Holding
                sts.HoldSessionId = holdSessionId;
                sts.HoldUntil = holdUntil;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("[CoupleSeat] Held {count} seats for showTime {showTimeId}",
                coupleSeats.Count, showTimeId);

            return coupleSeats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CoupleSeat] Error holding seats");
            throw;
        }
    }

    public async Task<List<Seat>> ReleaseSeatsAsync(Guid showTimeId, Seat seat)
    {
        var coupleSeats = await GetCoupleSeatsAsync(showTimeId, seat);
        var seatIds = coupleSeats.Select(s => s.SeatId).ToList();

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var showTimeSeats = await _context.ShowTimeSeats
                .Where(sts => sts.ShowTimeId == showTimeId && seatIds.Contains(sts.SeatId))
                .ToListAsync();

            foreach (var sts in showTimeSeats)
            {
                sts.Status = 0; // Available
                sts.HoldSessionId = null;
                sts.HoldUntil = null;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("[CoupleSeat] Released {count} seats for showTime {showTimeId}",
                coupleSeats.Count, showTimeId);

            return coupleSeats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CoupleSeat] Error releasing seats");
            throw;
        }
    }

    public async Task<List<Seat>> BookSeatsAsync(Guid showTimeId, Seat seat)
    {
        var coupleSeats = await GetCoupleSeatsAsync(showTimeId, seat);
        var seatIds = coupleSeats.Select(s => s.SeatId).ToList();

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var showTimeSeats = await _context.ShowTimeSeats
                .Where(sts => sts.ShowTimeId == showTimeId && seatIds.Contains(sts.SeatId))
                .ToListAsync();

            foreach (var sts in showTimeSeats)
            {
                sts.Status = 2; // Booked
                sts.HoldSessionId = null;
                sts.HoldUntil = null;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("[CoupleSeat] Booked {count} seats for showTime {showTimeId}",
                coupleSeats.Count, showTimeId);

            return coupleSeats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CoupleSeat] Error booking seats");
            throw;
        }
    }
}
