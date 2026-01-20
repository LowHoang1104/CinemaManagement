using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Cinema
{
    public Guid CinemaId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public Guid? LastUpdatedBy { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
