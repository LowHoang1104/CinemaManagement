using System.Net;
using System.Net.Mail;

namespace CinemaManagement.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmail(string toEmail, string otp, string fullName)
        {
            var smtp = _config.GetSection("SmtpSettings");

            var client = new SmtpClient
            {
                Host = smtp["Server"],
                Port = int.Parse(smtp["Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    smtp["UserName"],
                    smtp["Password"]
                )
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtp["SenderEmail"], smtp["SenderName"]),
                Subject = "Mã xác thực đặt lại mật khẩu - Beta Cinemas",
                Body = BuildEmailBody(fullName, otp),
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }

        private string BuildEmailBody(string fullName, string otp)
        {
            // Modern, responsive email HTML (inline CSS for compatibility)
            return $@"
<html>
  <body style=""margin:0;padding:0;font-family:Segoe UI, Roboto, Arial, sans-serif;background:#f4f6f8;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"" style=""padding:24px 12px;"">
          <table width=""600"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 8px 30px rgba(16,24,40,0.08);"">
            <tr>
              <td style=""padding:24px 28px 12px; text-align:left;"">
                <img src=""https://betacinemas.vn/Assets/Common/logo/logo.png"" alt=""Beta Cinemas"" width=""140"" style=""display:block; margin-bottom:12px;"" />
                <h2 style=""margin:0 0 8px;color:#0b5cb6;font-size:20px;"">Xin chào {System.Net.WebUtility.HtmlEncode(fullName)}</h2>
                <p style=""margin:0;color:#475569;font-size:14px;line-height:1.5;"">
                  Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản tại <strong>Beta Cinemas</strong>. Vui lòng sử dụng mã xác thực bên dưới để tiếp tục.
                </p>
              </td>
            </tr>

            <tr>
              <td align=""center"" style=""padding:18px 28px 8px;"">
                <div style=""display:inline-block;padding:18px 22px;border-radius:10px;background:linear-gradient(180deg,#fff 0%,#f7fbff 100%);border:1px solid #e6eefb;text-align:center;box-shadow:0 6px 18px rgba(11,92,182,0.08);"">
                  <div style=""font-size:13px;color:#556987;margin-bottom:6px;"">Mã xác thực của bạn</div>
                  <div style=""font-family: 'Courier New', monospace; font-weight:700; font-size:28px; letter-spacing:6px; color:#d33; margin:6px 0;"">{System.Net.WebUtility.HtmlEncode(otp)}</div>
                  <div style=""font-size:13px;color:#6b7280;margin-top:6px;"">Hết hạn trong <strong>5 phút</strong></div>
                </div>
              </td>
            </tr>

            <tr>
              <td style=""padding:8px 28px 20px;text-align:left;"">
                <p style=""margin:0 0 12px;color:#475569;font-size:13px;line-height:1.5;"">
                  Lưu ý:
                </p>
                <ul style=""margin:0 0 0 18px;padding:0;color:#475569;font-size:13px;line-height:1.5;"">
                  <li>Không chia sẻ mã này với bất kỳ ai.</li>
                  <li>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email.</li>
                </ul>
                <p style=""margin:16px 0 0;color:#9aa4b2;font-size:12px;"">Trân trọng,<br/>Beta Cinemas</p>
              </td>
            </tr>

            <tr>
              <td style=""background:#f7fafc;padding:12px 20px;text-align:center;font-size:12px;color:#94a3b8;"">
                © {DateTime.UtcNow.Year} Beta Cinemas — Bảo mật thông tin người dùng
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
        }
    }
}
