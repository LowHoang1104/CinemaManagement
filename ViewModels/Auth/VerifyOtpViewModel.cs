using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.ViewModels.Auth
{
    public class VerifyOtpViewModel
    {
        // kept for server-side usage (TempData provides the real email)
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        public string Otp { get; set; } = string.Empty;

        // Number of seconds remaining until expiry (provided by controller)
        public long RemainingSeconds { get; set; } = 0;

        // Masked contact to show (phone or email)
        public string ContactMasked { get; set; } = string.Empty;
    }
}
