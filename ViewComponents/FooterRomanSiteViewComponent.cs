using Microsoft.AspNetCore.Mvc;
using WebApp.Entities;

namespace WebApp.ViewComponents
{
    public class FooterRomanSiteViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("FooterRomanSite");  
        }
       
    }
}
