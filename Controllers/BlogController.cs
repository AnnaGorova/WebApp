using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class BlogController : Controller
    {
        
        public IActionResult BlogGridIndex()
        {
            ViewData["ActivePage"] = "BlogGrid";
            return View();
        }

        public IActionResult BlogDetailIndex()
        {
            ViewData["ActivePage"] = "BlogDetail";
            return View();
        }
    }
}
