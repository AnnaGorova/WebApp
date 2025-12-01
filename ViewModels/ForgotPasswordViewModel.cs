using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public string Email { get; set; }
    }
}
