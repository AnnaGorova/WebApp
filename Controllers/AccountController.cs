using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("LoginIn");
        }
        [HttpGet]
        public IActionResult LoginIn()
        {
            return View("LoginIn");
        }

        [HttpGet]
        public IActionResult RegisterIn()
        {
            return View("LoginIn");
        }
        
        
        
        [HttpPost]
        public IActionResult CheckUser()
        {
           throw new NotImplementedException();
        }

        public IActionResult RegisterUser()
        {
            throw new NotImplementedException();
        }
    }
}
