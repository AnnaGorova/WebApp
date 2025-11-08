using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Entities
{
    /// <summary>
    /// Модель для представлення навігаційного меню з підпунктами
    /// </summary>
    public class Navigate
    {
        /// <summary>
        /// Унікальний ідентифікатор навігаційного пункту
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int Id { get; set; }

        /// <summary>
        /// Назва навігаційного пункту (обов'язкове поле)
        /// Валідація: обов'язкове поле, максимальна довжина 100 символів
        /// </summary>
        [Required(ErrorMessage = "Назва пункту меню є обов'язковою")]
        [StringLength(100, ErrorMessage = "Назва не може перевищувати 100 символів")]
        [Display(Name = "Назва пункту")]
        public string Title { get; set; } = String.Empty;

        /// <summary>
        /// URL-адреса навігаційного пункту (обов'язкове поле)
        /// Валідація: обов'язкове поле, максимальна довжина 200 символів
        /// </summary>
        [Required(ErrorMessage = "URL-адреса є обов'язковою")]
        [StringLength(200, ErrorMessage = "URL-адреса не може перевищувати 200 символів")]
        [Display(Name = "URL-адреса")]
        public string Href { get; set; } = String.Empty;

        /// <summary>
        /// Порядок відображення в меню
        /// Валідація: значення має бути додатнім числом
        /// </summary>
        [Required(ErrorMessage = "Порядок відображення є обов'язковим")]
        [Range(1, int.MaxValue, ErrorMessage = "Порядок має бути додатнім числом")]
        [Display(Name = "Порядок відображення")]
        public int Order { get; set; }

        /// <summary>
        /// Ідентифікатор батьківського пункту меню
        /// Значення null означає, що це кореневий пункт меню
        /// Валідація: зовнішній ключ для ієрархічної структури
        /// </summary>
        [ForeignKey("ParentNavigate")]
        [Display(Name = "Батьківський пункт")]
        public int? ParentID { get; set; } = null;

        /// <summary>
        /// Батьківський навігаційний пункт (для навігаційної властивості)
        /// </summary>
        public virtual Navigate? ParentNavigate { get; set; }

        /// <summary>
        /// Колекція дочірніх навігаційних пунктів
        /// Використовується для побудови ієрархічного меню
        /// </summary>
        public virtual ICollection<Navigate> Childs { get; set; } = new List<Navigate>();
    }
}