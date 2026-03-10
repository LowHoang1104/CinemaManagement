using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class PasswordResetToken
{
    public Guid TokenId { get; set; }

    public Guid UserId { get; set; }

    public string Otpcode { get; set; } = null!;

    public DateTime ExpiryTime { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
