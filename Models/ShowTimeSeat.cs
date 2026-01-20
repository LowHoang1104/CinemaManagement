using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class ShowTimeSeat
{
    public Guid ShowTimeId { get; set; }

    public Guid SeatId { get; set; }

    public short Status { get; set; }

    public Guid? HoldByUserId { get; set; }

    public string? HoldSessionId { get; set; }

    public DateTime? HoldUntil { get; set; }

    public decimal? PriceOverride { get; set; }

    public virtual User? HoldByUser { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual ShowTime ShowTime { get; set; } = null!;
}
