using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Booking
{
    public Guid BookingId { get; set; }

    public Guid UserId { get; set; }

    public Guid ShowTimeId { get; set; }

    public string BookingCode { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpireAt { get; set; }

    public short Status { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual ShowTime ShowTime { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual User User { get; set; } = null!;
}
