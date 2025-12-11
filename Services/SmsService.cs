using Microsoft.Extensions.Configuration;

namespace WebApp.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly bool _isTestMode;

        public SmsService(ILogger<SmsService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _isTestMode = configuration.GetValue<bool>("SmsSettings:TestMode", true);
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                _logger.LogInformation($"Sending SMS to {phoneNumber}: {message}");

                if (_isTestMode)
                {
                    // Тестовий режим - лише логуємо
                    Console.WriteLine($"📱 SMS (TEST MODE) to {phoneNumber}: {message}");
                    await Task.Delay(100); // Імітація затримки
                    return true;
                }
                else
                {
                    // Реальна відправка SMS (приклад для Twilio)
                    // var client = new TwilioRestClient(accountSid, authToken);
                    // var result = await client.SendMessageAsync(fromPhone, phoneNumber, message);
                    // return result.Status == "queued";

                    _logger.LogWarning("SMS sending is disabled in production. Enable and configure SMS provider.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS");
                return false;
            }
        }


        public async Task<string> SendVerificationCodeAsync(string phoneNumber)
        {
            var code = new Random().Next(100000, 999999).ToString();
            var message = $"Ваш код підтвердження: {code} (тестовий режим)";

            var sent = await SendSmsAsync(phoneNumber, message);

            return sent ? code : null;
        }
    }
}