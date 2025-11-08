using Microsoft.AspNetCore.Mvc;
using WebApp.Entities;

namespace WebApp.ViewComponents
{
    public class FooterAboutViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("FooterAbout");  
        }
       
    }
}
