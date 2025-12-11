using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApp.Entities
{
    /// <summary>
    /// Представляє користувача системи
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        /// Унікальний ідентифікатор користувача
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Електронна пошта користувача
        /// </summary>
        [Required(ErrorMessage = "Електронна пошта обов'язкова")]
        [EmailAddress(ErrorMessage = "Невірний формат електронної пошти")]
        [MaxLength(128, ErrorMessage = "Електронна пошта не може перевищувати 255 символів")]
        [Column("Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// Логін користувача
        /// </summary>
        [Required(ErrorMessage = "Логін обов'язковий")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Логін повинен містити від 3 до 32 символів")]
        [Column("Login")]
        public string Login { get; set; }

        /// <summary>
        /// Пароль користувача
        /// </summary>
        [Required(ErrorMessage = "Пароль обов'язковий")]
        
        
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Дата створення запису користувача
        /// </summary>
        
        [DataType(DataType.DateTime)]
        [Column("DateOfCreat", TypeName = "datetime2")]
        [Display(Name = "Дата створення")]
        public DateTime DateOfCreat { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата останнього оновлення запису користувача
        /// </summary>

        [DataType(DataType.DateTime)]
        [Column("DateOfUpdated", TypeName = "datetime2")]
        [Display(Name = "Дата оновлення")]

        [ValidateNever]
        public DateTime? DateOfUpdated { get; set; } = null;






        public string? ResetPasswordCode { get; set; }
        public DateTime? ResetPasswordCodeExpires { get; set; }
        public bool ResetPasswordCodeUsed { get; set; }



        public string? GoogleId { get; set; } //  поле для Google авторизаці






        /// <summary>
        /// Номер телефону користувача
        /// </summary>
        [Phone(ErrorMessage = "Невірний формат телефону")]
        [StringLength(20, ErrorMessage = "Телефон не може перевищувати 20 символів")]
        [Column("PhoneNumber")]
        [Display(Name = "Номер телефону")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Чи підтверджено номер телефону
        /// </summary>
        [Column("PhoneNumberConfirmed")]
        [Display(Name = "Телефон підтверджено")]
        public bool PhoneNumberConfirmed { get; set; } = false;



        /// <summary>
        /// Перевіряє тип авторизації користувача
        /// </summary>
        public AuthType GetAuthType()
        {
            if (!string.IsNullOrEmpty(GoogleId))
                return AuthType.Google;

            if (!string.IsNullOrEmpty(PhoneNumber) && PhoneNumberConfirmed)
                return AuthType.Phone;

            return AuthType.EmailPassword;
        }

    }
}