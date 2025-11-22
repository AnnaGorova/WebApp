using Microsoft.AspNetCore.Mvc;
using WebApp.Db;
using WebApp.Models;

namespace WebApp.ViewComponents.Admin
{
    public class AdminHeaderViewComponent : ViewComponent
    {
        private readonly AgencyDBContext _agencyDBContext;

        private OptionModels _optionModel;
        public AdminHeaderViewComponent(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
            _optionModel = new OptionModels(agencyDBContext);
        }

        public IViewComponentResult Invoke()
        {
            return View("AdminHeader", _optionModel.GetSiteLogo());
        }
    }
}
