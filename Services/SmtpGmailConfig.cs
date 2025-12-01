namespace WebApp.Services
{
    public class SmtpGmailConfig
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool IsSslOrTls { get; set; } = true;
    }
}