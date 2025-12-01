using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Db;
using WebApp.Entities;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers
{

    public class AccountController : Controller
    {

        private readonly AgencyDBContext _agencyDBContext;

        private UserModel _userModel;
        private readonly IEmailService _emailService;

        public AccountController(AgencyDBContext agencyDBContext, IEmailService emailService)
        {
            _agencyDBContext = agencyDBContext;
            _userModel = new UserModel(_agencyDBContext);
            _emailService = emailService;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("LoginIn");
        }
        [HttpGet]
        public IActionResult LoginIn()
        {
            TempData.Remove("SuccessMessage");
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
                TempData["RegisteredUserName"] = newUser.Login;
                TempData["RegisteredUserEmail"] = newUser.Email;

                TempData.Keep("RegisteredUserName");
                TempData.Keep("RegisteredUserEmail");


                //// ПЕРЕВІРКА: адмін чи звичайний користувач
                //if (IsAdmin(newUser))
                //{
                //    return RedirectToAction("Index", "Admin");
                //}
                //else
                //{
                //    return RedirectToAction("Index", "Home");
                //}
                return RedirectToAction("RegistrationSuccess");

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
            TempData["LogoutSuccess"] = "Ви успішно вийшли з акаунту";
            return RedirectToAction("LoginIn");
        }




        private bool IsAdmin(User user)
        {

            return user.Email == "admin@admin.com";


        }





        //// GET: /Account/ForgotPassword
        //[HttpGet]
        //public IActionResult ForgotPassword()
        //{
        //    return View();
        //}

        //// POST: /Account/ForgotPassword
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ForgotPassword(string email)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(email))
        //        {
        //            TempData["ErrorMessage"] = "Будь ласка, введіть email";
        //            return View();
        //        }

        //        var user = _userModel.GetUserByEmail(email);
        //        if (user == null)
        //        {
        //            // Для безпеки не показуємо що користувача не існує
        //            TempData["SuccessMessage"] = "Якщо email існує, ми надішлемо інструкції для відновлення пароля";
        //            return View();
        //        }

        //        // Генеруємо токен
        //        var token = Guid.NewGuid().ToString();
        //        var expiryDate = DateTime.Now.AddMinutes(15);

        //        // Зберігаємо токен в базі
        //        var resetToken = new PasswordResetToken
        //        {
        //            Email = email,
        //            Token = token,
        //            ExpiryDate = expiryDate,
        //            IsUsed = false
        //        };

        //        _agencyDBContext.PasswordResetTokens.Add(resetToken);
        //        _agencyDBContext.SaveChanges();

        //        // Генеруємо URL для скидання пароля
        //        var resetLink = Url.Action("ResetPassword", "Account",
        //            new { token = token }, protocol: HttpContext.Request.Scheme);

        //        // Для тесту виведемо посилання в консоль
        //        Console.WriteLine($"RESET PASSWORD LINK: {resetLink}");
        //        TempData["DebugInfo"] = $"Тестове посилання: {resetLink}";
        //        TempData["SuccessMessage"] = "Інструкції для відновлення пароля надіслано на вашу email адресу.";

        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Сталася помилка: " + ex.Message;
        //        return View();
        //    }
        //}

        //// GET: /Account/ResetPassword
        //[HttpGet]
        //public IActionResult ResetPassword(string token)
        //{
        //    if (string.IsNullOrEmpty(token))
        //    {
        //        TempData["ErrorMessage"] = "Недійсне посилання для скидання пароля";
        //        return RedirectToAction("ForgotPassword");
        //    }

        //    // Перевіряємо токен
        //    var resetToken = _agencyDBContext.PasswordResetTokens
        //        .FirstOrDefault(t => t.Token == token && !t.IsUsed && t.ExpiryDate > DateTime.Now);

        //    if (resetToken == null)
        //    {
        //        TempData["ErrorMessage"] = "Недійсний або прострочений токен";
        //        return RedirectToAction("ForgotPassword");
        //    }

        //    var model = new ResetPasswordViewModel { Code = token };
        //    return View(model);
        //}

        //// POST: /Account/ResetPassword
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ResetPassword(ResetPasswordViewModel model)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return View(model);
        //        }

        //        // Перевіряємо токен
        //        var resetToken = _agencyDBContext.PasswordResetTokens
        //            .FirstOrDefault(t => t.Token == model.Code && !t.IsUsed && t.ExpiryDate > DateTime.Now);

        //        if (resetToken == null)
        //        {
        //            TempData["ErrorMessage"] = "Недійсний або прострочений токен";
        //            return RedirectToAction("ForgotPassword");
        //        }

        //        // Знаходимо користувача
        //        var user = _userModel.GetUserByEmail(resetToken.Email);
        //        if (user == null)
        //        {
        //            TempData["ErrorMessage"] = "Користувача не знайдено";
        //            return View(model);
        //        }

        //        // Оновлюємо пароль
        //        user.PasswordHash = SecurePasswordHasher.Hash(model.Password);
        //        user.DateOfUpdated = DateTime.Now;

        //        // Позначаємо токен як використаний
        //        resetToken.IsUsed = true;

        //        _agencyDBContext.SaveChanges();

        //        TempData["SuccessMessage"] = "Пароль успішно змінено! Тепер ви можете увійти з новим паролем.";
        //        return RedirectToAction("LoginIn", "Account");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Помилка при зміні пароля: " + ex.Message;
        //        return View(model);
        //    }
        //}













        //[HttpGet]
        //public IActionResult ForgotPassword()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        //        if (user != null)
        //        {
        //            // Генеруємо код підтвердження (6 цифр)
        //            var resetCode = GenerateResetCode();

        //            // Зберігаємо код в базі
        //            user.ResetPasswordCode = resetCode;
        //            user.ResetPasswordCodeExpires = DateTime.UtcNow.AddMinutes(15);

        //            _context.Users.Update(user);
        //            await _context.SaveChangesAsync();

        //            // Відправляємо email з кодом
        //            var emailSent = await _emailService.SendPasswordResetEmailAsync(
        //                user.Email,
        //                resetCode,
        //                user.Login ?? "Користувач"  // Використовуємо Login замість UserName
        //            );

        //            if (emailSent)
        //            {
        //                TempData["SuccessMessage"] = "Код відновлення відправлено на вашу email адресу";
        //                return RedirectToAction("ResetPassword", new { email = user.Email });
        //            }
        //            else
        //            {
        //                TempData["ErrorMessage"] = "Помилка при відправці email. Спробуйте пізніше.";
        //            }
        //        }
        //        else
        //        {
        //            // Для безпеки не показуємо, що email не знайдено
        //            TempData["SuccessMessage"] = "Якщо email існує, код відновлення буде відправлено";
        //            return RedirectToAction("Login");
        //        }
        //    }

        //    return View(model);
        //}

        //[HttpGet]
        //public IActionResult ResetPassword(string email)
        //{
        //    var model = new ResetPasswordViewModel { Email = email };
        //    return View(model);
        //}

        //[HttpPost]
        //public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        //        if (user != null &&
        //            user.ResetPasswordCode == model.Code &&
        //            user.ResetPasswordCodeExpires > DateTime.UtcNow)
        //        {
        //            // Код вірний і не прострочений - змінюємо пароль
        //            user.PasswordHash = HashPassword(model.Password);
        //            user.ResetPasswordCode = null;
        //            user.ResetPasswordCodeExpires = null;

        //            _context.Users.Update(user);
        //            await _context.SaveChangesAsync();

        //            TempData["SuccessMessage"] = "Пароль успішно змінено!";
        //            return RedirectToAction("Login");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("Code", "Невірний або прострочений код підтвердження");
        //        }
        //    }

        //    return View(model);
        //}

        //private string GenerateResetCode()
        //{
        //    var random = new Random();
        //    return random.Next(100000, 999999).ToString(); // 6-значний код
        //}

        //private string HashPassword(string password)
        //{
        //    // Використовуйте вашу існуючу логіку хешування
        //    return BCrypt.Net.BCrypt.HashPassword(password);
        //}








        // === FORGOT PASSWORD METHODS ===

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            Console.WriteLine($"=== FORGOT PASSWORD DEBUG ===");

            if (ModelState.IsValid)
            {
                var user = _userModel.GetUserByEmail(model.Email);

                if (user != null)
                {
                    var resetCode = new Random().Next(100000, 999999).ToString();

                    // Зберігаємо код
                    user.ResetPasswordCode = resetCode;
                    user.ResetPasswordCodeExpires = DateTime.UtcNow.AddMinutes(15);
                    _agencyDBContext.SaveChanges();

                    Console.WriteLine($"✅ Code saved: {resetCode}");

                    // ПЕРЕВІРКА EMAIL SERVICE
                    Console.WriteLine($"📧 EmailService: {_emailService != null}");

                    if (_emailService != null)
                    {
                        Console.WriteLine("🔄 Attempting to send email...");
                        try
                        {
                            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                                user.Email, resetCode, user.Login);

                            Console.WriteLine($"📧 Email sent result: {emailSent}");

                            if (emailSent)
                            {
                                TempData["SuccessMessage"] = "Код відправлено на ваш email";
                            }
                            else
                            {
                                TempData["SuccessMessage"] = $"Код відновлення: {resetCode} (email не відправлено)";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Email error: {ex.Message}");
                            TempData["SuccessMessage"] = $"Код відновлення: {resetCode} (помилка email: {ex.Message})";
                        }
                    }
                    else
                    {
                        Console.WriteLine("❌ EmailService is NULL!");
                        TempData["SuccessMessage"] = $"Код відновлення: {resetCode} (EmailService не налаштовано)";
                    }

                    return RedirectToAction("ResetPassword", new { email = user.Email });
                }
            }

            return View(model);
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            var model = new ResetPasswordViewModel { Email = email };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userModel.GetUserByEmail(model.Email);

                if (user != null &&
                    user.ResetPasswordCode == model.Code &&
                    user.ResetPasswordCodeExpires > DateTime.UtcNow)
                {
                    // Код вірний і не прострочений - змінюємо пароль
                    user.PasswordHash = SecurePasswordHasher.Hash(model.Password);
                    user.ResetPasswordCode = null;
                    user.ResetPasswordCodeExpires = null;

                    _agencyDBContext.Users.Update(user);
                    await _agencyDBContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Пароль успішно змінено!";
                    return RedirectToAction("LoginIn");
                }
                else
                {
                    ModelState.AddModelError("Code", "Невірний або прострочений код підтвердження");
                }
            }

            return View(model);
        }

        private string GenerateResetCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // 6-значний код
        }

        //private bool IsAdmin(User user)
        //{
        //    return user.Email == "admin@admin.com";
        //}
    }
}








    

