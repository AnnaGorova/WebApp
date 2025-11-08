using Microsoft.AspNetCore.Mvc;
using WebApp.Entities;

namespace WebApp.ViewComponents
{
    public class FooterGetInTouchViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("FooterGetInTouch");  
        }
       
    }
}
