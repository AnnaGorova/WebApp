namespace WebApp.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageSrc { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty; 
        public PostStatuses PostStatuses { get; set; } = PostStatuses.Created;    
        public DateTime DataOfCreated { get; set; } = DateTime.Now;
        public DateTime DataOfUpdated { get; set; } = DateTime.Now;
        public DateTime DataOfPublished { get; set; } = DateTime.Now;





        // Додайте навігаційні властивості
        public virtual ICollection<PostTags> PostTags { get; set; } = new List<PostTags>();
        public virtual ICollection<PostCategories> PostCategories { get; set; } = new List<PostCategories>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
