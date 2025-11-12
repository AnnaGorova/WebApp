using WebApp.Db;
using WebApp.Entities;

namespace WebApp.Models
{
    public class TagModel
    {
        private AgencyDBContext _agencyDBContext;
        public TagModel(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
        }
        public IEnumerable<Tag> GetNotEmptyTags()
        {
            return _agencyDBContext.Tags
                  .Where(tag => _agencyDBContext.PostTags.Any(pt => pt.TagId == tag.Id))
                  .ToList();
        }
    }
}
