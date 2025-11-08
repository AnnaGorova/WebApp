using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Entities
{
    /// <summary>
    /// Модель для зберігання налаштувань та динамічних опцій системи
    /// Використовується для керування соціальними посиланнями, контактною інформацією та іншими налаштуваннями
    /// </summary>
    public class Option
    {
        /// <summary>
        /// Унікальний ідентифікатор опції
        /// </summary>
        [Key]
        [Display(Name = "ID")]
        public int Id { get; set; }

        /// <summary>
        /// Назва опції для відображення
        /// Валідація: обов'язкове поле, максимальна довжина 100 символів
        /// </summary>
        [Required(ErrorMessage = "Назва опції є обов'язковою")]
        [StringLength(128, ErrorMessage = "Назва не може перевищувати 128 символів")]
        [Display(Name = "Назва опції")]
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Унікальний ключ для ідентифікації опції в коді
        /// Валідація: обов'язкове поле, максимальна довжина 50 символів
        /// </summary>
        [Required(ErrorMessage = "Ключ опції є обов'язковим")]
        [StringLength(50, ErrorMessage = "Ключ не може перевищувати 50 символів")]
        [Display(Name = "Ключ опції")]
        public string Key { get; set; } = String.Empty;

        /// <summary>
        /// Значення опції (URL, текст, число тощо)
        /// Валідація: обов'язкове поле, максимальна довжина 500 символів
        /// </summary>
        [Required(ErrorMessage = "Значення опції є обов'язковим")]
        [StringLength(500, ErrorMessage = "Значення не може перевищувати 500 символів")]
        [Display(Name = "Значення опції")]
        public string Value { get; set; } = String.Empty;

        /// <summary>
        /// Група опцій для групування (наприклад: "social-link", "contact-info", "site-settings")
        /// Валідація: обов'язкове поле, максимальна довжина 50 символів
        /// </summary>
        [Required(ErrorMessage = "Група опцій є обов'язковою")]
        [StringLength(50, ErrorMessage = "Група не може перевищувати 50 символів")]
        [Display(Name = "Група опцій")]
        public string Relation { get; set; } = String.Empty;

        /// <summary>
        /// Порядок відображення в межах групи
        /// Валідація: обов'язкове поле, додатнє число
        /// </summary>
        [Required(ErrorMessage = "Порядок відображення є обов'язковим")]
        [Range(0, int.MaxValue, ErrorMessage = "Порядок має бути невід'ємним числом")]
        [Display(Name = "Порядок відображення")]
        public int Order { get; set; }

        /// <summary>
        /// Вказує, чи є опція системною (не можна видаляти/редагувати через UI)
        /// Значення за замовчуванням: false
        /// </summary>
        [DefaultValue(false)]
        [Display(Name = "Системна опція")]
        public bool IsSystem { get; set; }
    }
}