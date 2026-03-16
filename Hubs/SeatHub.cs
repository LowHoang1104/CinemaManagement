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

    // GUIDs for seat statuses (match scripts)
    private static readonly Guid SeatStatusActive = Guid.Parse("550e8400-e29b-41d4-a716-000000000001");
    private static readonly Guid SeatStatusInactive = Guid.Parse("550e8400-e29b-41d4-a716-000000000002");

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
            s.Status = 2; // held/reserved status (domain-specific)
            s.HoldUntil = holdUntil;
            s.HoldSessionId = Context.ConnectionId;

            var seat = await _db.Seats.FindAsync(s.SeatId);
            if (seat != null)
            {
                seat.SeatStatusId = SeatStatusInactive;
            }
        }

        await _db.SaveChangesAsync();

        // Notify other clients in the showtime group
        await Clients.GroupExcept(showTimeId.ToString(), Context.ConnectionId)
            .SendAsync("SeatsHeld", new { showTimeId, seatIds, holdUntil = holdUntil.ToString("o") });
    }

    public async Task ReleaseSeats(Guid showTimeId, List<Guid> seatIds)
    {
        var sts = await _db.ShowTimeSeats
            .Where(s => s.ShowTimeId == showTimeId && seatIds.Contains(s.SeatId))
            .ToListAsync();

        foreach (var s in sts)
        {
            s.Status = 0; // available
            s.HoldUntil = null;
            s.HoldSessionId = null;

            var seat = await _db.Seats.FindAsync(s.SeatId);
            if (seat != null)
            {
                seat.SeatStatusId = SeatStatusActive;
            }
        }

        await _db.SaveChangesAsync();

        await Clients.Group(showTimeId.ToString()).SendAsync("SeatsReleased", new { showTimeId, seatIds });
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
        var held = await _db.ShowTimeSeats.Where(s => s.HoldSessionId == connId).ToListAsync();

        var groupedByShow = held.GroupBy(s => s.ShowTimeId);

        foreach (var kv in groupedByShow)
        {
            var showTimeId = kv.Key;
            var seatIds = kv.Select(s => s.SeatId).ToList();

            foreach (var s in kv)
            {
                s.Status = 0;
                s.HoldSessionId = null;
                s.HoldUntil = null;

                var seat = await _db.Seats.FindAsync(s.SeatId);
                if (seat != null)
                    seat.SeatStatusId = SeatStatusActive;
            }

            await _db.SaveChangesAsync();

            await Clients.Group(showTimeId.ToString()).SendAsync("SeatsReleased", new { showTimeId, seatIds });
        }

        await base.OnDisconnectedAsync(exception);
    }
}
