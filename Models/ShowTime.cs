using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class ShowTime
{
    public Guid ShowTimeId { get; set; }

    public Guid MovieId { get; set; }

    public Guid RoomId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public decimal BasePrice { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public Guid? LastUpdatedBy { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Movie Movie { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<ShowTimeSeat> ShowTimeSeats { get; set; } = new List<ShowTimeSeat>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
