using Microsoft.Extensions.Caching.Memory;
using System.IO; // Додайте цей
using System.Text.RegularExpressions; // Додайте цей

namespace WebApp.Services
{
    public class MockSmsService : ISmsService
    {
        private readonly ILogger<MockSmsService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;

        public MockSmsService(
            ILogger<MockSmsService> logger,
            IMemoryCache cache,
            IEmailService emailService = null)
        {
            _logger = logger;
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // Логуємо "відправку" SMS
                _logger.LogInformation($"📱 [DEMO SMS] To: {phoneNumber}");
                _logger.LogInformation($"   Message: {message}");

                // Зберігаємо останнє повідомлення в кеш
                _cache.Set($"LastSMS_{phoneNumber}", message, TimeSpan.FromMinutes(10));

                // Записуємо в файл (повний шлях)
                var logEntry = $"[{DateTime.Now:HH:mm:ss}] To: {phoneNumber} | Message: {message}\n";

                // Створюємо папку якщо немає
                var logDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                var logPath = Path.Combine(logDir, "sms_log.txt");
                await System.IO.File.AppendAllTextAsync(logPath, logEntry);

                // Якщо це код підтвердження
                if (message.Contains("код") || message.Contains("code"))
                {
                    var code = ExtractCodeFromMessage(message);
                    if (!string.IsNullOrEmpty(code))
                    {
                        _cache.Set($"SMSCode_{phoneNumber}", code, TimeSpan.FromMinutes(5));
                        _logger.LogInformation($"   🔐 Код: {code}");
                    }
                }

                await Task.Delay(300); // Імітація затримки
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка Mock SMS");
                return false;
            }
        }

        // Метод для інтерфейсу
        public async Task<string> SendVerificationCodeAsync(string phoneNumber)
        {
            var code = new Random().Next(100000, 999999).ToString();
            var message = $"Ваш код підтвердження: {code} (демо)";

            await SendSmsAsync(phoneNumber, message);
            return code;
        }

        private string ExtractCodeFromMessage(string message)
        {
            // Тепер Regex доступний через using System.Text.RegularExpressions;
            var match = Regex.Match(message, @"\b\d{6}\b");
            return match.Success ? match.Value : null;
        }
    }
}