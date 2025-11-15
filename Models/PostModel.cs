using WebApp.Db;
using WebApp.Entities;

namespace WebApp.Models
{
    public class PostModel
    {
        private AgencyDBContext _agencyDBContext;

        public PostModel(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
        }

        public Post? GetPostBySlug(string slug)
        {
            return _agencyDBContext.Posts.FirstOrDefault(p => p.Slug == slug);
        }
    }
}