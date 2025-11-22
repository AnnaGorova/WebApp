using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebApp.Db;
using WebApp.Entities;
using WebApp.Helpers;
using WebApp.Models;

namespace WebApp.Controllers
{

    public class AccountController : Controller
    {

        private readonly AgencyDBContext _agencyDBContext;

        private UserModel _userModel;

        public AccountController(AgencyDBContext agencyDBContext)
        {
                _agencyDBContext = agencyDBContext;
                _userModel = new UserModel(_agencyDBContext);
        }


        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("LoginIn");
        }
        [HttpGet]
        public IActionResult LoginIn()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RegisterIn()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RegistrationSuccess()
        {
            return View();
        }



        [HttpPost]
        public IActionResult CheckUser(string email, string password)
        {
            User? user = _userModel.GetUserByEmail(email);

            //string passwordHash = SecurePasswordHasher.Hash(password);
            if (user != null && SecurePasswordHasher.Verify(password, user.PasswordHash))
            {

                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email)
                };
                var identity = new ClaimsIdentity(claims, "CookieAuth", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(identity));



                // ПЕРЕВІРКА: адмін чи звичайний користувач
                if (IsAdmin(user))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                // Authentication failed
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View("LoginIn", new ErrorViewModel() { ErrorMessage="User or Password incorrect" });
            }
        }

        [HttpPost]
        public IActionResult RegisterUser(string email, string login, string password, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
                {
                    return View("RegisterIn", new ErrorViewModel() { ErrorMessage = "Всі поля мають бути заповнені" });
                }

                if (!email.Contains("@") || !email.Contains("."))
                { return View("RegisterIn", new ErrorViewModel() { ErrorMessage = "Некоректний формат Email" }); 
                }
                               

                if (password != confirmPassword)
				{
					return View("RegisterIn", new ErrorViewModel() { ErrorMessage = "Паролі не співпадають" });
				}

                var existingUserByLogin = _agencyDBContext.Users.FirstOrDefault(u => u.Login == login);

                if (existingUserByLogin != null)
				{
					return View("RegisterIn", new ErrorViewModel() { ErrorMessage = "Користувач з таким логіном вже існує" });
				}

                var newUser = new User
				{
					Email = email.Trim(),
					Login = login.Trim(),
					PasswordHash = SecurePasswordHasher.Hash(password),
					DateOfCreat = DateTime.Now,
					DateOfUpdated = null
				};  

                _agencyDBContext.Users.Add(newUser);
				_agencyDBContext.SaveChanges();



				// Автоматичний вхід після реєстрації
                var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, newUser.Email),
                    new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
                    new Claim("Login", newUser.Login)

				};

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

				HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity)).Wait();

                TempData["SuccessMessage"] = "Реєстрація пройшла успішно! Ласкаво просимо!";




                // ПЕРЕВІРКА: адмін чи звичайний користувач
                if (IsAdmin(newUser))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }


            }
            catch (Exception ex)
			{
				return View("RegisterIn", new ErrorViewModel() { ErrorMessage = ex.Message });
			}
		}

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();

            return RedirectToAction("LoginIn");
        }




        private bool IsAdmin(User user)
        {
            
            return user.Email == "admin@admin.com";

            
        }



    }
    
}
