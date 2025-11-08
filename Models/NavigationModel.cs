using Microsoft.EntityFrameworkCore;
using WebApp.Db;
using WebApp.Entities;

namespace WebApp.Models
{
    public class NavigationModel
    {
        private AgencyDBContext _agencyDBContext;
        public NavigationModel(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
        }

        public IEnumerable<Navigate> GetNavigates()
        {
            return _agencyDBContext.Navigates
                .Include(n => n.Childs) // додаємо включення  дочірніх  елементів
                .OrderBy(n => n.Order);
        }


        public IEnumerable<Navigate> GetNavigationTree()
        {
            var allNavigates = _agencyDBContext.Navigates
                .Include(n => n.Childs)
                .ToList();

            return BuildTree(allNavigates, null);
        }

        // Рекурсивний метод для побудови дерева
        private List<Navigate> BuildTree(List<Navigate> allItems, int? parentId)
        {
            return allItems
                .Where(n => n.ParentID == parentId)
                .OrderBy(n => n.Order)
                .Select(n => new Navigate
                {
                    Id = n.Id,
                    Title = n.Title,
                    Href = n.Href,
                    Order = n.Order,
                    ParentID = n.ParentID,
                    Childs = BuildTree(allItems, n.Id) // Рекурсивно будуємо дочірні елементи
                })
                .ToList();
        }
    }
}


        //public IEnumerable<Navigate> GetNavigationThree()
        //{
        //    var threeRoot = _agencyDBContext.Navigates
        //        .Where(n => n.ParentID == null)
        //        .OrderBy(n => n.Order)
        //        .ToList();
        //    var allChilds = _agencyDBContext.Navigates
        //        .Where(n => n.ParentID != null)
        //        .OrderBy(n => n.Order)
        //        .ToList(); 
        //    foreach (var oneParent in threeRoot)
        //    {
        //        foreach (var oneChild in allChilds)
        //        {
        //            if (oneChild.ParentID == oneParent.Id)
        //            {
        //                oneParent.Childs.Add(oneChild);
        //            }
        //        }
        //    }
        //    return threeRoot;
        //}
