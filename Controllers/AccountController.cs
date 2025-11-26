using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebApp.Db;
using WebApp.Entities;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.ViewModels;

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
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));



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
                return View("LoginIn", new ErrorViewModel() { ErrorMessage = "User or Password incorrect" });
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
                {
                    return View("RegisterIn", new ErrorViewModel() { ErrorMessage = "Некоректний формат Email" });
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





        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    TempData["ErrorMessage"] = "Будь ласка, введіть email";
                    return View();
                }

                var user = _userModel.GetUserByEmail(email);
                if (user == null)
                {
                    // Для безпеки не показуємо що користувача не існує
                    TempData["SuccessMessage"] = "Якщо email існує, ми надішлемо інструкції для відновлення пароля";
                    return View();
                }

                // Генеруємо токен
                var token = Guid.NewGuid().ToString();
                var expiryDate = DateTime.Now.AddMinutes(15);

                // Зберігаємо токен в базі
                var resetToken = new PasswordResetToken
                {
                    Email = email,
                    Token = token,
                    ExpiryDate = expiryDate,
                    IsUsed = false
                };

                _agencyDBContext.PasswordResetTokens.Add(resetToken);
                _agencyDBContext.SaveChanges();

                // Генеруємо URL для скидання пароля
                var resetLink = Url.Action("ResetPassword", "Account",
                    new { token = token }, protocol: HttpContext.Request.Scheme);

                // Для тесту виведемо посилання в консоль
                Console.WriteLine($"RESET PASSWORD LINK: {resetLink}");
                TempData["DebugInfo"] = $"Тестове посилання: {resetLink}";
                TempData["SuccessMessage"] = "Інструкції для відновлення пароля надіслано на вашу email адресу.";

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Сталася помилка: " + ex.Message;
                return View();
            }
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Недійсне посилання для скидання пароля";
                return RedirectToAction("ForgotPassword");
            }

            // Перевіряємо токен
            var resetToken = _agencyDBContext.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && !t.IsUsed && t.ExpiryDate > DateTime.Now);

            if (resetToken == null)
            {
                TempData["ErrorMessage"] = "Недійсний або прострочений токен";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Перевіряємо токен
                var resetToken = _agencyDBContext.PasswordResetTokens
                    .FirstOrDefault(t => t.Token == model.Token && !t.IsUsed && t.ExpiryDate > DateTime.Now);

                if (resetToken == null)
                {
                    TempData["ErrorMessage"] = "Недійсний або прострочений токен";
                    return RedirectToAction("ForgotPassword");
                }

                // Знаходимо користувача
                var user = _userModel.GetUserByEmail(resetToken.Email);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Користувача не знайдено";
                    return View(model);
                }

                // Оновлюємо пароль
                user.PasswordHash = SecurePasswordHasher.Hash(model.Password);
                user.DateOfUpdated = DateTime.Now;

                // Позначаємо токен як використаний
                resetToken.IsUsed = true;

                _agencyDBContext.SaveChanges();

                TempData["SuccessMessage"] = "Пароль успішно змінено! Тепер ви можете увійти з новим паролем.";
                return RedirectToAction("LoginIn", "Account");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка при зміні пароля: " + ex.Message;
                return View(model);
            }
        }


    }
}
    

