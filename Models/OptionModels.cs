using WebApp.Db;
using WebApp.Entities;

namespace WebApp.Models
{
    public class OptionModels
    {
        private AgencyDBContext _agencyDBContext;
        public OptionModels(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
        }

        public IEnumerable<Option> GetSocialLinks()
        {
            return _agencyDBContext.Options.Where(o => o.Relation == "social-link").OrderBy(o => o.Order).ToList();
        }

        public IEnumerable<Option> GetContactInformation()
        {
            return _agencyDBContext.Options.Where(o => o.Relation == "contact-info").OrderBy(o => o.Order).ToList();
        }

        internal object GetSiteLogo()
        {
            // Реалізація пошуку логотипу в базі даних
            // Спробуємо знайти логотип в базі даних
            var logo = _agencyDBContext.Options
                        .FirstOrDefault(o => o.Name == "site-logo" || o.Relation == "site-logo");


            if (logo == null)
            {
                return new Option
                {
                    Id = 0,
                    IsSystem = true,
                    Name = "site-logo",
                    Value = "Щастинка щастя",
                    Key = "<i class=\"fa fa-user-tie me-2\"></i>",
                    Relation = "site-logo",
                    Order = 0
                };
            }

            return logo;
        }
    }
}
