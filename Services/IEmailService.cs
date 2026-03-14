using System.Threading.Tasks;

namespace CinemaManagement.Services
{
    public interface IEmailService
    {
        Task SendOtpEmail(string toEmail, string otp, string fullName);
    }
}
