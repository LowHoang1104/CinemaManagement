using CinemaManagement.Models;
using CinemaManagement.ViewModels.Auth;
using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Threading.Tasks;
namespace CinemaManagement.Services
{
    public record RegistrationRequest(string FullName, string? Phone, string Email, string Password, bool AgreeTerms);
    public record RegistrationResult(bool Success, Guid? UserId = null, string[]? Errors = null);

    public record LoginRequest(string Credential, string Password, bool RememberMe);

    public record LoginResult(bool Success, string? ErrorMessage = null, User? User = null);

    public interface IAuthService
    {
        Task<RegistrationResult> RegisterAsync(RegistrationRequest request);

        Task<LoginResult> LoginAsync(LoginRequest request);

        Task<User?> LoginGoogleAsync(string email);

        Task<(DateTime? ExpiryUtc, string? MaskedContact)> SendResetOtpAsync(string email);

        Task<(bool Success, Guid? UserId, string Error)> VerifyOtpAsync(string email, string otp);

        Task<bool> ResetPasswordAsync(Guid userId, string newPassword);

        // Profile operations (no avatar / no upload)
        Task<ProfileViewModel?> GetUserProfileAsync(Guid userId);

        // Update profile (no avatar file parameter)
        Task<(bool Success, string? Error)> UpdateUserProfileAsync(Guid userId, ProfileViewModel model);
    
}
}
