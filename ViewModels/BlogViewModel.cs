using WebApp.Entities;

namespace WebApp.ViewModels
{
    public class BlogViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();

        public IEnumerable<Post> Posts { get; set; } = new List<Post>();

        public IEnumerable<Post> RecentPosts { get; set; } = new List<Post>();
        public Post? CurrentPost { get; set; }



        //властивості для пагінації
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string CurrentCategory { get; set; }

        public string CurrentTag { get; set; }
    }
}
