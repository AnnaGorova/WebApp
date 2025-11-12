namespace WebApp.Entities
{
    public class PostCategories
    {
      
        public int PostId { get; set; }

        public virtual Post Post { get; set; }

        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }
    }
}
