using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models;

public class PasswordResetToken
{
    [Key]
    public Guid TokenId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required, StringLength(6)]
    public string OTPCode { get; set; } = null!;

    [Required]
    public DateTime ExpiryTime { get; set; }

    [Required]
    public bool IsUsed { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    // Navigation
    public virtual User? User { get; set; }
}