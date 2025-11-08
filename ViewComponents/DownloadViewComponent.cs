using Microsoft.AspNetCore.Mvc;

namespace WebApp.ViewComponents
{
    public class DownloadViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("Download");  
        }
    }
}
