using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Seat
{
    public Guid SeatId { get; set; }

    public Guid RoomId { get; set; }

    public string SeatCode { get; set; } = null!;

    public int RowLabel { get; set; }

    public int ColNumber { get; set; }

    public string SeatType { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public Guid? LastUpdatedBy { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<ShowTimeSeat> ShowTimeSeats { get; set; } = new List<ShowTimeSeat>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
