using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Ticket
{
    public Guid TicketId { get; set; }

    public Guid BookingId { get; set; }

    public Guid ShowTimeId { get; set; }

    public Guid SeatId { get; set; }

    public decimal UnitPrice { get; set; }

    public string TicketCode { get; set; } = null!;

    public bool Status { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Seat Seat { get; set; } = null!;

    public virtual ShowTime ShowTime { get; set; } = null!;
}
