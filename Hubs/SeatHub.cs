using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.Hubs;

public class SeatHub : Hub
{
    private readonly CinemaManagementContext _db;
    private readonly ICoupleSeatService _coupleSeatService;
    private readonly ILogger<SeatHub> _logger;

    public SeatHub(CinemaManagementContext db, ICoupleSeatService coupleSeatService, ILogger<SeatHub> logger)
    {
        _db = db;
        _coupleSeatService = coupleSeatService;
        _logger = logger;
    }

    public async Task JoinShowTime(Guid showTimeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, showTimeId.ToString());
    }

    public async Task HoldSeats(Guid showTimeId, List<Guid> seatIds, int holdSeconds)
    {
        var now = DateTime.UtcNow;
        var holdUntil = now.AddSeconds(holdSeconds);

        try
        {
            // For each selected seat, handle couple seat logic
            var allSeatsToHold = new HashSet<Guid>();
            var seatsWithPairs = new List<(Seat primary, List<Seat> couple)>();

            foreach (var seatId in seatIds)
            {
                if (allSeatsToHold.Contains(seatId))
                    continue; // Already processed as part of a couple

                var seat = await _db.Seats.FirstOrDefaultAsync(s => s.SeatId == seatId);
                if (seat == null)
                {
                    _logger.LogWarning("[HoldSeats] Seat not found: {seatId}", seatId);
                    throw new HubException($"Ghế không tồn tại: {seatId}");
                }

                var coupleSeats = await _coupleSeatService.GetCoupleSeatsAsync(showTimeId, seat);
                seatsWithPairs.Add((seat, coupleSeats));

                foreach (var s in coupleSeats)
                    allSeatsToHold.Add(s.SeatId);
            }

            // Now check status of all seats to hold
            var sts = await _db.ShowTimeSeats
                .Where(s => s.ShowTimeId == showTimeId && allSeatsToHold.Contains(s.SeatId))
                .ToListAsync();

            foreach (var st in sts)
            {
                // If seat is holding but hold has expired → auto release
                if (st.Status == 1 && st.HoldUntil != null && st.HoldUntil < now)
                {
                    st.Status = 0; // Reset to Available
                    st.HoldUntil = null;
                    st.HoldSessionId = null;
                }

                // If seat is being held by someone else → block
                if (st.Status == 1 && st.HoldSessionId != Context.ConnectionId)
                {
                    throw new HubException("Ghế đang được người khác giữ");
                }

                // If seat is already booked → block
                if (st.Status == 2)
                {
                    throw new HubException("Ghế đã được đặt");
                }

                // Set seat to holding (Status = 1)
                st.Status = 1;
                st.HoldUntil = holdUntil;
                st.HoldSessionId = Context.ConnectionId;
            }

            await _db.SaveChangesAsync();

            // Notify other clients about held seats (including coupled seats)
            await Clients.GroupExcept(showTimeId.ToString(), Context.ConnectionId)
                .SendAsync("SeatsHeld", new
                {
                    showTimeId,
                    seatIds = allSeatsToHold.ToList(),
                    holdUntil = holdUntil.ToString("o")
                });

            _logger.LogInformation("[HoldSeats] Held {count} seats (including couples)", allSeatsToHold.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HoldSeats] Error");
            throw new HubException($"Lỗi giữ ghế: {ex.Message}");
        }
    }
    public async Task ReleaseSeats(Guid showTimeId, List<Guid> seatIds)
    {
        try
        {
            // For each selected seat, handle couple seat logic
            var allSeatsToRelease = new HashSet<Guid>();

            foreach (var seatId in seatIds)
            {
                if (allSeatsToRelease.Contains(seatId))
                    continue; // Already processed

                var seat = await _db.Seats.FirstOrDefaultAsync(s => s.SeatId == seatId);
                if (seat == null)
                    continue;

                var coupleSeats = await _coupleSeatService.GetCoupleSeatsAsync(showTimeId, seat);
                foreach (var s in coupleSeats)
                    allSeatsToRelease.Add(s.SeatId);
            }

            var sts = await _db.ShowTimeSeats
                .Where(s => s.ShowTimeId == showTimeId && allSeatsToRelease.Contains(s.SeatId))
                .ToListAsync();

            foreach (var s in sts)
            {
                s.Status = 0; // Reset to Available
                s.HoldUntil = null;
                s.HoldSessionId = null;
            }

            await _db.SaveChangesAsync();

            // Broadcast SeatsReleased event with all related seats
            await Clients.Group(showTimeId.ToString()).SendAsync("SeatsReleased", new
            {
                showTimeId,
                seatIds = allSeatsToRelease.ToList()
            });

            _logger.LogInformation("[ReleaseSeats] Released {count} seats (including couples)", allSeatsToRelease.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReleaseSeats] Error");
            throw;
        }
    }

    public async Task ClearHold(Guid showTimeId, List<Guid> seatIds)
    {
        try
        {
            // For each selected seat, handle couple seat logic
            var allSeatsToClear = new HashSet<Guid>();

            foreach (var seatId in seatIds)
            {
                if (allSeatsToClear.Contains(seatId))
                    continue;

                var seat = await _db.Seats.FirstOrDefaultAsync(s => s.SeatId == seatId);
                if (seat == null)
                    continue;

                var coupleSeats = await _coupleSeatService.GetCoupleSeatsAsync(showTimeId, seat);
                foreach (var s in coupleSeats)
                    allSeatsToClear.Add(s.SeatId);
            }

            var sts = await _db.ShowTimeSeats
                .Where(s => s.ShowTimeId == showTimeId && allSeatsToClear.Contains(s.SeatId))
                .ToListAsync();

            foreach (var s in sts)
            {
                // Only clear hold from this session, don't change seat status
                if (s.HoldSessionId == Context.ConnectionId)
                {
                    s.HoldSessionId = null;
                    s.HoldUntil = null;
                    // Keep Status as is
                }
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ClearHold] Error");
            throw;
        }
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext != null && httpContext.Request.Query.TryGetValue("showtime", out var showtimeValues))
        {
            var showtime = showtimeValues.FirstOrDefault();
            if (Guid.TryParse(showtime, out var showtimeId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, showtimeId.ToString());
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Release any holds held by this connection that are still active
        var connId = Context.ConnectionId;
        var held = await _db.ShowTimeSeats
            .Where(s => s.HoldSessionId == connId)
            .Include(s => s.Seat)
            .ToListAsync();

        var groupedByShow = held.GroupBy(s => s.ShowTimeId);

        foreach (var kv in groupedByShow)
        {
            var showTimeId = kv.Key;
            var seatIds = kv.Select(s => s.SeatId).ToList();

            foreach (var s in kv)
            {
                s.Status = 0; // Reset to Available
                s.HoldSessionId = null;
                s.HoldUntil = null;
            }

            await _db.SaveChangesAsync();

            // Broadcast SeatsReleased event
            await Clients.Group(showTimeId.ToString()).SendAsync("SeatsReleased", new { showTimeId, seatIds });
        }

        await base.OnDisconnectedAsync(exception);
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