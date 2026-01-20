using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid BookingId { get; set; }

    public string Method { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? PaidAt { get; set; }

    public short Status { get; set; }

    public string? ProviderRef { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
