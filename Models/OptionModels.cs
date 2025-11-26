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

        public IEnumerable<Option> GetAllOptions()
        {
            return _agencyDBContext.Options.ToList();
        }

        public Option GetOptionById(int optionId)
        {
            return _agencyDBContext.Options.FirstOrDefault(o => o.Id == optionId);
        }

        public IEnumerable<string> GetUniqueRelations()
        {
            return _agencyDBContext.Options
                 .Select(o => o.Relation)
                 .Where(r => !string.IsNullOrEmpty(r))
                 .Distinct()
                 .ToList();
        }




        public void AddNewRelation(string newRelation)
        {
            // Не створюємо нову опцію, бо Relation - це лише поле групування
            // Можна просто логувати або нічого не робити
            Console.WriteLine($"Додано нове значення Relation: {newRelation}");

            // Якщо потрібно, можна зберігати список доступних Relations в окремій таблиці
            // Але в поточній структурі це лише поле в Options
        }



        public void UpdateOption(Option updatedOption)
        {
            if (updatedOption == null)
            {
                throw new ArgumentNullException(nameof(updatedOption), "Updated option cannot be null");
            }

            var existingOption = _agencyDBContext.Options.Find(updatedOption.Id);

            if (existingOption == null)
            {
                throw new Exception($"Опція з ID {updatedOption.Id} не знайдена в базі даних.");
            }

            if (!existingOption.IsSystem)
            {
                existingOption.Name = updatedOption.Name;
                existingOption.Key = updatedOption.Key;
                existingOption.Value = updatedOption.Value;
                existingOption.Relation = updatedOption.Relation;
                existingOption.Order = updatedOption.Order;

                _agencyDBContext.SaveChanges();
            }
            else
            {
                throw new Exception("Системні опції не можна редагувати.");
            }
        }
    }
}