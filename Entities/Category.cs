using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageSrc { get; set; } = string.Empty;

        /// Foreign Key - зберігає ID батьківської категорії (число)
        public int? ParentID { get; set; }

        // Navigation Property - посилається на об'єкт батьківської категорії  
        [ForeignKey("ParentID")]
        public virtual Category? ParentCategory
        {
            get; set;
        }

            // Це поле ігноруємо - воно для зворотньої сумісності
            [NotMapped]
        public int? ParentCategoryId { get; set; }

        public virtual ICollection<PostCategories> PostCategories { get; set; } = new List<PostCategories>();

    }
}

