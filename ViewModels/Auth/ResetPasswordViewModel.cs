using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.ViewModels.Auth
{
    public class ResetPasswordViewModel
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [RegularExpression(@"(?=.*[A-Za-z])(?=.*\d).{6,}", ErrorMessage = "Mật khẩu phải tối thiểu 6 ký tự và gồm chữ và số")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
