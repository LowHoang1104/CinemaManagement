using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0)")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{6,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ cái và 1 chữ số")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bạn phải đồng ý với điều khoản sử dụng")]
        [Display(Name = "Đồng ý điều khoản")]
        public bool AgreeTerms { get; set; }


        // Tự validate để kiểm soát thứ tự lỗi
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ưu tiên kiểm tra độ dài trước
            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult("Mật khẩu là bắt buộc.", new[] { nameof(Password) });
            }
            else if (Password.Length < 6)
            {
                yield return new ValidationResult("Mật khẩu phải từ 6 ký tự trở lên.", new[] { nameof(Password) });
            }
            // Sau đó mới kiểm tra nội dung
            else if (!System.Text.RegularExpressions.Regex.IsMatch(Password, @"^(?=.*[A-Za-z])(?=.*\d).{6,}$"))
            {
                yield return new ValidationResult("Mật khẩu phải chứa ít nhất 1 chữ cái và 1 chữ số.", new[] { nameof(Password) });
            }
        }
    }
}
