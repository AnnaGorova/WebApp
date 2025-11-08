using Microsoft.AspNetCore.Mvc;

namespace WebApp.ViewComponents
{
    public class TopSearchViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("TopSearch");
        }
    }
}
