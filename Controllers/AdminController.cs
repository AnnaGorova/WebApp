using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
	[Authorize]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
			{
				return View();
			}
			
			return RedirectToAction("LoginIn", "Account");
		}





		public IActionResult Categories()
		{
			return View();
		}	



	}
}
