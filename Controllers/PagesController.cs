using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class PagesController : Controller
    {
        public IActionResult PricingPlan()
        {
            ViewData["ActivePage"] = "PricingPlan";
            return View();
        }

        public IActionResult OurFeatures()
        {
            ViewData["ActivePage"] = "OurFeatures";
            return View();
        }

        public IActionResult TeamMembers()
        {
            ViewData["ActivePage"] = "TeamMembers";
            return View();
        }

        public IActionResult Testimonial()
        {
            ViewData["ActivePage"] = "Testimonial";
            return View();
        }

        public IActionResult FreeQuote()
        {
            ViewData["ActivePage"] = "FreeQuote";
            return View();
        }
    }
}
