using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaManagement.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Hubs;

public class SeatHub : Hub
{
    private readonly CinemaManagementContext _db;

    public SeatHub(CinemaManagementContext db)
    {
        _db = db;
    }

    public async Task JoinShowTime(Guid showTimeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, showTimeId.ToString());
    }

    public async Task HoldSeats(Guid showTimeId, List<Guid> seatIds, int holdSeconds)
    {
        var now = DateTime.UtcNow;
        var holdUntil = now.AddSeconds(holdSeconds);

        var sts = await _db.ShowTimeSeats
            .Where(s => s.ShowTimeId == showTimeId && seatIds.Contains(s.SeatId))
            .ToListAsync();

        foreach (var s in sts)
        {
            // If seat is holding but hold has expired → auto release
            if (s.Status == 1 && s.HoldUntil != null && s.HoldUntil < now)
            {
                s.Status = 0; // Reset to Available
                s.HoldUntil = null;
                s.HoldSessionId = null;
            }

            // If seat is being held by someone else → block
            if (s.Status == 1 && s.HoldSessionId != Context.ConnectionId)
            {
                throw new HubException("Ghế đang được người khác giữ");
            }

            // If seat is already booked → block
            if (s.Status == 2)
            {
                throw new HubException("Ghế đã được đặt");
            }

            // Set seat to holding (Status = 1)
            s.Status = 1;
            s.HoldUntil = holdUntil;
            s.HoldSessionId = Context.ConnectionId;
        }

        await _db.SaveChangesAsync();

        // Notify other clients about held seats
        await Clients.GroupExcept(showTimeId.ToString(), Context.ConnectionId)
            .SendAsync("SeatsHeld", new
            {
                showTimeId,
                seatIds,
                holdUntil = holdUntil.ToString("o")
            });
    }
    public async Task ReleaseSeats(Guid showTimeId, List<Guid> seatIds)
    {
        var sts = await _db.ShowTimeSeats
            .Where(s => s.ShowTimeId == showTimeId && seatIds.Contains(s.SeatId))
            .ToListAsync();

        foreach (var s in sts)
        {
            s.Status = 0; // Reset to Available
            s.HoldUntil = null;
            s.HoldSessionId = null;
        }

        await _db.SaveChangesAsync();

        // Broadcast SeatsReleased event
        await Clients.Group(showTimeId.ToString()).SendAsync("SeatsReleased", new { showTimeId, seatIds });
    }

    public async Task ClearHold(Guid showTimeId, List<Guid> seatIds)
    {
        var sts = await _db.ShowTimeSeats
            .Where(s => s.ShowTimeId == showTimeId && seatIds.Contains(s.SeatId))
            .ToListAsync();

        foreach (var s in sts)
        {
            // Only clear hold from this session, don't change seat status
            if (s.HoldSessionId == Context.ConnectionId)
            {
                s.HoldSessionId = null;
                s.HoldUntil = null;
                // Keep Status = 2 so seat remains "holding" visually
            }
        }

        await _db.SaveChangesAsync();
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