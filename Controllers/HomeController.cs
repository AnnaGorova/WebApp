using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;


namespace WebApp.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {

        }

        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Home";
            return View();
        }
    }
}
