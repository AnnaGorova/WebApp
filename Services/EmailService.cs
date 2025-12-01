using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace WebApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpGmailConfig _smtpConfig;

        public EmailService(IOptions<SmtpGmailConfig> smtpConfig)
        {
            _smtpConfig = smtpConfig.Value;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetCode, string userName)
        {
            try
            {
                Console.WriteLine($"=== EMAIL SENDING START ===");
                Console.WriteLine($"To: {toEmail}");
                Console.WriteLine($"Code: {resetCode}");
                Console.WriteLine($"From: {_smtpConfig.Email}");
                Console.WriteLine($"Host: {_smtpConfig.Host}:{_smtpConfig.Port}");

                using (var smtpClient = new SmtpClient(_smtpConfig.Host, _smtpConfig.Port))
                {
                    smtpClient.Credentials = new NetworkCredential(_smtpConfig.Email, _smtpConfig.Password);
                    smtpClient.EnableSsl = _smtpConfig.IsSslOrTls;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_smtpConfig.Email, "Your App Name"),
                        Subject = "Відновлення пароля",
                        Body = GeneratePasswordResetEmailBody(userName, resetCode),
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    Console.WriteLine("Attempting to send email...");
                    await smtpClient.SendMailAsync(mailMessage);
                    Console.WriteLine("✅ Email sent successfully!");

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email sending failed: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                return false;
            }
        }

        private string GeneratePasswordResetEmailBody(string userName, string resetCode)
        {
            return $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: #007bff; color: white; padding: 20px; text-align: center;'>
        <h1>Відновлення пароля</h1>
    </div>
    <div style='padding: 20px; background: #f9f9f9;'>
        <p>Шановний(а) <strong>{userName}</strong>,</p>
        <p>Ми отримали запит на відновлення пароля для вашого облікового запису.</p>
        <p>Використовуйте наступний код для відновлення пароля:</p>
        <div style='font-size: 24px; font-weight: bold; color: #007bff; text-align: center; margin: 20px 0; padding: 10px; background: white; border: 2px dashed #007bff;'>
            {resetCode}
        </div>
        <p>Цей код дійсний протягом <strong>15 хвилин</strong>.</p>
        <p>Якщо ви не запитували відновлення пароля, проігноруйте цей лист.</p>
    </div>
    <div style='text-align: center; padding: 20px; font-size: 12px; color: #666;'>
        <p>© 2024 Your App Name. Всі права захищені.</p>
    </div>
</div>";
        }
    }
}

