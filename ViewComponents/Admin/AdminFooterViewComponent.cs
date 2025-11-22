using Microsoft.AspNetCore.Mvc;
using WebApp.Db;
using WebApp.Models;

namespace WebApp.ViewComponents.Admin
{
    public class AdminFooterViewComponent : ViewComponent
    {
        private readonly AgencyDBContext _agencyDBContext;

        private OptionModels _optionModel;
        public AdminFooterViewComponent(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
            _optionModel = new OptionModels(agencyDBContext);
        }

        public IViewComponentResult Invoke()
        {
            return View("AdminFooter", _optionModel.GetSiteLogo());
        }
    }
}
