using CinemaManagement.Data;
using CinemaManagement.Helpers;
using CinemaManagement.Models;
using CinemaManagement.Services;
using CinemaManagement.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CinemaManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly CinemaManagementContext _db;
        private readonly IEmailService _emailService;
       
        public AuthService(CinemaManagementContext db, IEmailService emailService )
        {
            _db = db;
            _emailService = emailService;           
        }


        public async Task<RegistrationResult> RegisterAsync(RegistrationRequest request)
        {
            if (request == null) return new RegistrationResult(false, null, new[] { "Yêu cầu không hợp lệ." });
          
            var emailNorm = request.Email.Trim().ToLowerInvariant();
            var phoneNorm = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

            // Chỉ kiểm tra unique
            if (await _db.Users.AnyAsync(u => u.Email.ToLower() == emailNorm))
                return new RegistrationResult(false, null, new[] { "Email đã được sử dụng." });

            if (!string.IsNullOrWhiteSpace(phoneNorm) &&
                await _db.Users.AnyAsync(u => u.Phone != null && u.Phone == phoneNorm))
                return new RegistrationResult(false, null, new[] { "Số điện thoại đã được sử dụng." });

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Email = emailNorm,
                Phone = phoneNorm,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = null,
                LastUpdatedAt = DateTime.UtcNow,
                LastUpdatedBy = null,
            };

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                const string customerRoleName = "Customer";
                var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == customerRoleName.ToLower());
                if (role == null)
                {
                    role = new Role { RoleId = Guid.NewGuid(), Name = customerRoleName };
                    _db.Roles.Add(role);
                    await _db.SaveChangesAsync();
                }

                // assign role via navigation collection
                user.Roles.Add(role);

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
                return new RegistrationResult(true, user.UserId, Array.Empty<string>());
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                return new RegistrationResult(false, null, new[] { "Lỗi lưu dữ liệu. Vui lòng thử lại." });
            }
        }



        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Credential) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResult(false, "Vui lòng nhập đầy đủ thông tin.");
            }

            var credentialNorm = request.Credential.Trim().ToLowerInvariant();

            // Tìm user theo email hoặc phone
            var user = await _db.Users
                .Include(u => u.Roles)  // Load roles luôn
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == credentialNorm ||
                    (u.Phone != null && u.Phone == credentialNorm));

            if (user == null)
            {
                return new LoginResult(false, "Email/Số điện thoại hoặc mật khẩu không đúng.");
            }

            // Kiểm tra trạng thái tài khoản
            if (user.Status != 1)
            {
                return new LoginResult(false, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ hỗ trợ.");
            }

            // Kiểm tra mật khẩu
            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new LoginResult(false, "Email/Số điện thoại hoặc mật khẩu không đúng.");
            }

            return new LoginResult(true, null, user);
        }


        public async Task<User?> LoginGoogleAsync(string email)
        {
            var emailNorm = email.Trim().ToLowerInvariant();

            return await _db.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == emailNorm);
        }



        public async Task<(DateTime? ExpiryUtc, string? MaskedContact)> SendResetOtpAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (null, null);

            var emailNorm = email.Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == emailNorm);
            if (user == null) return (null, null);

            string otp = new Random().Next(100000, 999999).ToString();

            var token = new PasswordResetToken
            {
                TokenId = Guid.NewGuid(),
                UserId = user.UserId,
                Otpcode = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            _db.PasswordResetTokens.Add(token);
            await _db.SaveChangesAsync();

            await _emailService.SendOtpEmail(user.Email, otp ,user.FullName);

            // compute masked contact for UI
            string masked;
            if (!string.IsNullOrWhiteSpace(user.Phone))
            {
                // mask phone like 098****123 (keep first 3 and last 3 if possible)
                var p = user.Phone.Trim();
                if (p.Length > 6)
                    masked = p.Substring(0, 3) + new string('*', Math.Max(3, p.Length - 6)) + p.Substring(p.Length - 3);
                else
                    masked = p;
            }
            else
            {
                // mask email: keep first char(s) and domain
                var parts = user.Email.Split('@');
                if (parts.Length == 2)
                {
                    var name = parts[0];
                    var domain = parts[1];
                    if (name.Length <= 2)
                        masked = name + "@" + domain;
                    else
                        masked = name.Substring(0, Math.Min(3, name.Length)) + new string('*', Math.Max(2, name.Length - 3)) + "@" + domain;
                }
                else
                {
                    masked = user.Email;
                }
            }

            return (token.ExpiryTime, masked);
        }

        public async Task<(bool Success, Guid? UserId, string Error)> VerifyOtpAsync(string email, string otp)
        {
            var emailNorm = email.Trim().ToLower();

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == emailNorm);
            if (user == null) return (false, null, "Email không tồn tại");

            var token = await _db.PasswordResetTokens
                .Where(x => x.UserId == user.UserId &&
                            x.Otpcode == otp &&
                            !x.IsUsed &&
                            x.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (token == null)
                return (false, null, "OTP không đúng hoặc đã hết hạn");

            token.IsUsed = true;
            await _db.SaveChangesAsync();

            return (true, user.UserId, "");
        }


        public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            user.LastUpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }




        // New profile methods 
        public async Task<ProfileViewModel?> GetUserProfileAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return null;

            return new ProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.Phone ?? string.Empty
            };
        }

        public async Task<(bool Success, string? Error)> UpdateUserProfileAsync(Guid userId, ProfileViewModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return (false, "Người dùng không tồn tại");

            var emailNorm = model.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            if (await _db.Users.AnyAsync(u => u.UserId != userId && u.Email.ToLower() == emailNorm))
            {
                return (false, "Email đã được sử dụng bởi tài khoản khác.");
            }

            var phoneNorm = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
            if (!string.IsNullOrWhiteSpace(phoneNorm) &&
                await _db.Users.AnyAsync(u => u.UserId != userId && u.Phone != null && u.Phone == phoneNorm))
            {
                return (false, "Số điện thoại đã được sử dụng bởi tài khoản khác.");
            }

            if (!string.IsNullOrWhiteSpace(phoneNorm) && !Regex.IsMatch(phoneNorm, @"^0\d{9}$"))
            {
                return (false, "Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0).");
            }

            user.FullName = model.FullName?.Trim() ?? user.FullName;
            user.Email = emailNorm;
            user.Phone = phoneNorm;
            user.LastUpdatedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException)
            {
                return (false, "Lỗi lưu dữ liệu. Vui lòng thử lại.");
            }
        }

      


    }
}
