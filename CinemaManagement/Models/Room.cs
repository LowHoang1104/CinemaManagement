using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Room
{
    public Guid RoomId { get; set; }

    public Guid CinemaId { get; set; }

    public string Name { get; set; } = null!;

    public int TotalRows { get; set; }

    public int TotalCols { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public Guid? LastUpdatedBy { get; set; }

    public virtual Cinema Cinema { get; set; } = null!;

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
}
