using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class SeatStatus
{
    public Guid SeatStatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
