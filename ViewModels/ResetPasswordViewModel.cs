using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Код підтвердження обов'язковий")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [DataType(DataType.Password)]
       
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }
    }
}