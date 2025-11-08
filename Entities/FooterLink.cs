using System.ComponentModel.DataAnnotations;

namespace WebApp.Entities
{
    /// <summary>
    /// Модель для представлення популярних посилань у футері сайту
    /// Використовується для відображення посилань у розділі "Popular Links"
    /// </summary>
    public class FooterLink
    {

        /// <summary>
        /// Унікальний ідентифікатор посилання
        /// </summary>
        [Key]
        [Display(Name = "ID")]
        public int Id { get; set; }
        /// <summary>
        /// Текст посилання для відображення
        /// Валідація: обов'язкове поле, максимальна довжина 50 символів
        /// </summary>
        [Required(ErrorMessage = "Назва посилання є обов'язковою")]
        [StringLength(50, ErrorMessage = "Назва не може перевищувати 50 символів")]
        [Display(Name = "Назва посилання")]
        public string Title { get; set; } = String.Empty;

        /// <summary>
        /// URL-адреса посилання
        /// Валідація: обов'язкове поле, максимальна довжина 200 символів
        /// </summary>
        [Required(ErrorMessage = "URL-адреса є обов'язковою")]
        [StringLength(200, ErrorMessage = "URL-адреса не може перевищувати 200 символів")]
        [Url(ErrorMessage = "Вкажіть коректну URL-адресу")]
        [Display(Name = "URL-адреса")]
        public string Href { get; set; } = String.Empty;

        /// <summary>
        /// Порядок відображення посилань у списку
        /// Валідація: обов'язкове поле, додатнє число
        /// </summary>
        [Required(ErrorMessage = "Порядок відображення є обов'язковим")]
        [Range(1, 100, ErrorMessage = "Порядок має бути в діапазоні від 1 до 100")]
        [Display(Name = "Порядок відображення")]
        public int Order { get; set; }
    }
}
