namespace WebApp.Services
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);

        Task<string> SendVerificationCodeAsync(string phoneNumber);
    }
}
