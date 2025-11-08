using System.ComponentModel.DataAnnotations;

namespace WebApp.Entities
{
    /// <summary>
    /// Модель для зберігання повідомлень від клієнтів
    /// </summary>
    public class ClientMessage
    {
        /// <summary>
        /// Унікальний ідентифікатор повідомлення
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ім'я користувача (обов'язкове поле)
        /// Валідація: обов'язкове поле з повідомленням про помилку українською
        /// </summary>
        [Required(ErrorMessage = "Будь ласка заповніть поле ім'я")]
        [Display(Name = "Ім'я")]
        [StringLength(100, ErrorMessage = "Ім'я не може бути довше 100 символів")]
        public string UserName { get; set; } = String.Empty;

        /// <summary>
        /// Email адреса користувача (обов'язкове поле)
        /// Валідація: 
        /// - Обов'язкове поле
        /// - Формат email адреси
        /// - Максимальна довжина 128 символів
        /// </summary>
        [Required(ErrorMessage = "Будь ласка заповніть поле Email")]
        [EmailAddress(ErrorMessage = "Вкажіть валідний Email адресу")]
        [MaxLength(128, ErrorMessage = "Email може бути не довше 128 символів")]
        [Display(Name = "Email адреса")]
        public string UserEmail { get; set; } = String.Empty;

        /// <summary>
        /// Тема повідомлення (необов'язкове поле)
        /// </summary>
        [Display(Name = "Тема повідомлення")]
        [StringLength(200, ErrorMessage = "Тема не може бути довшою за 200 символів")]
        public string Subject { get; set; } = String.Empty;

        /// <summary>
        /// Текст повідомлення (обов'язкове поле)
        /// Валідація: обов'язкове поле з повідомленням про помилку українською
        /// </summary>
        [Required(ErrorMessage = "Будь ласка заповніть поле - тіло листа")]
        [Display(Name = "Повідомлення")]
        [StringLength(5000, ErrorMessage = "Повідомлення не може бути довшим за 5000 символів")]
        public string Message { get; set; } = String.Empty;

        /// <summary>
        /// Дата та час створення повідомлення
        /// Значення за замовчуванням: поточний час
        /// </summary>
        [Display(Name = "Дата створення")]
        public DateTime DateOfCreated { get; set; } = DateTime.Now;

        /// <summary>
        /// Прапорець, що вказує чи було дане повідомлення оброблено
        /// Значення за замовчуванням: false (не оброблено)
        /// </summary>
        [Display(Name = "Відповідь надіслано")]
        public bool IsAnswered { get; set; } = false; // Виправлено назву з IdAnswered на IsAnswered
    }
}