using Microsoft.AspNetCore.Mvc;
using WebApp.Db;
using WebApp.Entities;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.ViewComponents
{
    public class TopBarViewComponent : ViewComponent
    {
        private readonly AgencyDBContext _agencyDBContext;

        private OptionModels _optionModel;
        public TopBarViewComponent(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
            _optionModel = new OptionModels(agencyDBContext);
        }

        public IViewComponentResult Invoke()
        {
            TopBarViewModels topBarViewModel = new TopBarViewModels();

            topBarViewModel.SocialLinks = _optionModel.GetSocialLinks();
            topBarViewModel.ContactInformation = _optionModel.GetContactInformation();

            return View("TopBar", topBarViewModel);
        }
    }
}