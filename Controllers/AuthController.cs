using CinemaManagement.Data;
using CinemaManagement.Helpers;
using CinemaManagement.Models;
using CinemaManagement.Services;
using CinemaManagement.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Security.Claims;

namespace CinemaManagement.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;


        [HttpGet]
        public IActionResult Login()
        {
            ViewData["ResetSuccess"] = TempData["ResetSuccess"] as string;

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginReq = new Services.LoginRequest(model.Credential, model.Password, model.RememberMe);
            var result = await _authService.LoginAsync(loginReq);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Đăng nhập thất bại");
                return View(model);
            }

            // create claims and sign in (unchanged)
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, result.User!.UserId.ToString()),
        new Claim(ClaimTypes.Name, result.User.FullName),
        new Claim(ClaimTypes.Email, result.User.Email),
    };

            foreach (var role in result.User.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
            });

            // Phân quyền redirect theo role
            var isAdmin = result.User.Roles.Any(r => r.Name == "Admin");

            if (isAdmin)
            {
                //return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                return View("AdminLogin");
            }

            // Customer
            //return RedirectToAction("Index", "Home");
            return View("CustomerLogin");
        }



        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse), "Auth");
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                TempData["LoginError"] = "Xác thực Google thất bại.";
                return RedirectToAction(nameof(Login));
            }

            var email = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["LoginError"] = "Không lấy được email từ Google.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _authService.LoginGoogleAsync(email);

            if (user == null)
            {
                TempData["LoginError"] =
                    "Tài khoản Google của bạn chưa đăng ký với Beta Cinema.";
                return RedirectToAction(nameof(Login));
            }

            await SignInUser(user);

            return RedirectUserByRole(user);
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email),
    };

            foreach (var role in user.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }
        private IActionResult RedirectUserByRole(User user)
        {
            if (user.Roles.Any(r => r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                return View("AdminLogin");

            return View("CustomerLogin");
        }





        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new RegistrationRequest(
                FullName: model.FullName,
                Phone: model.PhoneNumber,
                Email: model.Email,
                Password: model.Password,
                AgreeTerms: model.AgreeTerms
             );

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                // Xử lý lỗi từ service → gắn vào đúng trường nếu có thể
                foreach (var error in result.Errors ?? Array.Empty<string>())
                {
                    // Phân loại lỗi và gắn vào key chính xác
                    if (error.Contains("Email đã được sử dụng") || error.Contains("Email đã tồn tại"))
                    {
                        ModelState.AddModelError("Email", error);
                    }
                    else if (error.Contains("Số điện thoại đã được sử dụng") || error.Contains("Số điện thoại đã tồn tại"))
                    {
                        ModelState.AddModelError("PhoneNumber", error);
                    }
                    else if (error.Contains("Họ tên"))
                    {
                        ModelState.AddModelError("FullName", error);
                    }
                    else
                    {
                        // Các lỗi khác gắn vào string.Empty nếu không xác định được
                        ModelState.AddModelError(string.Empty, error);
                    }
                }

                // Nếu có lỗi → trả về view với model để giữ dữ liệu + hiển thị lỗi dưới từng field
                return View(model);
            }

            TempData["RegisterSuccess"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (expiryUtc, maskedContact) = await _authService.SendResetOtpAsync(model.Email);

            if (expiryUtc == null)
            {
                ModelState.AddModelError(nameof(ForgotPasswordViewModel.Email), "Email không tồn tại");
                return View(model);
            }

            // Store necessary data in TempData (kept server-side between requests)
            TempData["OtpExpiryUtc"] = expiryUtc.Value.ToString("o", CultureInfo.InvariantCulture);
            TempData["OtpContactMasked"] = model.Email?.Trim() ?? string.Empty;
            TempData["OtpEmail"] = model.Email;

            return RedirectToAction(nameof(VerifyOtp));
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            // If user navigated directly, redirect back to forgot password
            var email = TempData.Peek("OtpEmail") as string;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(ForgotPassword));

            var expiryStr = TempData.Peek("OtpExpiryUtc") as string;
            DateTime expiryUtc = DateTime.UtcNow;

            // Parse exact round-trip ("o") format — if parsing fails, fallback to UtcNow + 5min to avoid immediate expiry
            if (!string.IsNullOrEmpty(expiryStr)
                && DateTime.TryParseExact(expiryStr, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedExpiry))
            {
                expiryUtc = parsedExpiry;
            }
            else
            {
                // thời gian xác thực otp
                expiryUtc = DateTime.UtcNow.AddMinutes(5);
            }

            var remaining = (long)Math.Max(0, (expiryUtc - DateTime.UtcNow).TotalSeconds);

            var model = new VerifyOtpViewModel
            {
                Email = email,
                RemainingSeconds = remaining,
                ContactMasked = TempData.Peek("OtpContactMasked") as string ?? string.Empty
            };

            // Keep TempData so POST can still read the real email and expiry
            TempData.Keep("OtpEmail");
            TempData.Keep("OtpExpiryUtc");
            TempData.Keep("OtpContactMasked");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            // Email is retrieved from TempData (not from URL or visible query)
            var email = TempData.Peek("OtpEmail") as string;
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Phiên đã hết. Vui lòng yêu cầu lại mã OTP.");
                return RedirectToAction(nameof(ForgotPassword));
            }

            var result = await _authService.VerifyOtpAsync(email, model.Otp);

            if (!result.Success)
            {
                // Attach error to the Otp field so the view shows it under the OTP inputs
                ModelState.AddModelError(nameof(VerifyOtpViewModel.Otp), result.Error);

                // Keep TempData so user can retry
                TempData.Keep("OtpEmail");
                TempData.Keep("OtpExpiryUtc");
                TempData.Keep("OtpContactMasked");
                return View(model);
            }

            // remove TempData entries once used
            TempData.Remove("OtpEmail");
            TempData.Remove("OtpExpiryUtc");
            TempData.Remove("OtpContactMasked");

            return RedirectToAction("ResetPassword", new { userId = result.UserId });
        }

        [HttpGet]
        public IActionResult ResetPassword(Guid userId)
        {
            return View(new ResetPasswordViewModel { UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ViewModels.Auth.ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updated = await _authService.ResetPasswordAsync(model.UserId, model.NewPassword);
            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật mật khẩu. Vui lòng thử lại.");
                return View(model);
            }

            TempData["ResetSuccess"] = "Cập nhật mật khẩu thành công. Vui lòng đăng nhập lại.";

            return RedirectToAction(nameof(Login));
        }


        // GET: load profile (no avatar)
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var model = await _authService.GetUserProfileAsync(userId);
            if (model == null) return NotFound();

            ViewData["ProfileUpdateSuccess"] = TempData["ProfileUpdateSuccess"];
            return View(model);
        }

        // POST: update profile (no avatar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
            {
                ModelState.AddModelError(string.Empty, "Phiên đã hết. Vui lòng đăng nhập lại.");
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, error) = await _authService.UpdateUserProfileAsync(userId, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Không thể cập nhật thông tin. Vui lòng thử lại.");
                return View(model);
            }

            TempData["ProfileUpdateSuccess"] = "Cập nhật thành công.";
            return RedirectToAction(nameof(Profile));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ViewModels.Auth.ChangePasswordViewModel model)
        {
            // Ensure user is authenticated
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var emailClaim = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId) || string.IsNullOrEmpty(emailClaim))
            {
                return Json(new { success = false, error = "Phiên đã hết. Vui lòng đăng nhập lại." });
            }

            if (!ModelState.IsValid)
            {
                // return first error message
                var first = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault()
                            ?? "Dữ liệu không hợp lệ.";
                return Json(new { success = false, error = first });
            }

            // Verify current password by trying to login with email + current password.
            var loginReq = new Services.LoginRequest(emailClaim, model.CurrentPassword, false);
            var loginResult = await _authService.LoginAsync(loginReq);
            if (!loginResult.Success || loginResult.User == null || loginResult.User.UserId != userId)
            {
                return Json(new { success = false, error = "Mật khẩu hiện tại không đúng." });
            }

            // All good: update password
            var updated = await _authService.ResetPasswordAsync(userId, model.NewPassword);
            if (!updated)
            {
                return Json(new { success = false, error = "Không thể cập nhật mật khẩu. Vui lòng thử lại." });
            }

            return Json(new { success = true, message = "Cập nhật mật khẩu thành công." });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }



       


    }
}