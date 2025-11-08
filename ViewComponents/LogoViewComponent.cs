using Microsoft.AspNetCore.Mvc;
using WebApp.Db;
using WebApp.Entities;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.ViewComponents
{
    public class LogoViewComponent : ViewComponent
    {
        private readonly AgencyDBContext _agencyDBContext;

        private OptionModels _optionModel;
        public LogoViewComponent(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
            _optionModel = new OptionModels(agencyDBContext);
        }

        public IViewComponentResult Invoke()
        {
            return View("Logo", _optionModel.GetSiteLogo());
        }
    }


    //private Option _logoOption;
    //public LogoViewComponent()
    //{
    //    _logoOption = new Option()
    //    {
    //        Id = 1,
    //        IsSystem = true,
    //        Name = "site-logo",
    //        Value = "Щастинка щастя",
    //        Key = "<i class=\"fa fa-user-tie me-2\"></i>"
    //    };
    //}
    //public IViewComponentResult Invoke()
    //{
    //    return View("Logo", _logoOption);
    //}
}



    
