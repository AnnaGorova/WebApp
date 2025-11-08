using Microsoft.AspNetCore.Mvc;
using WebApp.Entities;

namespace WebApp.Controllers
{
    public class AboutController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "About";
            return View();
        }
        [HttpGet]
        public IActionResult ContactUs()
        {
            ViewData["ActivePage"] = "ContactUs";
            return View();
        }

        [HttpPost]
        public ViewResult SaveClientMessage(ClientMessage clientMessage)
        {
            if (ModelState.IsValid)
            {
                // save to db
                return View("Thanks", clientMessage);
            }
            else
            {
                return View("ContactUs");
            }
        }
    }
}
