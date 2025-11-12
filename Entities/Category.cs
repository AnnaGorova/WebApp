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

        [ForeignKey("ParentNavigate")]
        [Display(Name = "Батьківський пункт")]
        public int? ParentID { get; set; } = null;

        /// <summary>
        /// Батьківський навігаційний пункт (для навігаційної властивості)
        /// </summary>
        public virtual Category? ParentCategory { get; set; }
       


        public virtual ICollection<PostCategories> PostCategories { get; set; } = new List<PostCategories>();

    }
}

