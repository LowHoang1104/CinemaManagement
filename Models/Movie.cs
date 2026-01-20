using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Movie
{
    public Guid MovieId { get; set; }

    public string Title { get; set; } = null!;

    public int DurationMin { get; set; }

    public string? Description { get; set; }

    public string? PosterUrl { get; set; }

    public int? AgeRating { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public Guid? LastUpdatedBy { get; set; }

    public virtual ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
}
