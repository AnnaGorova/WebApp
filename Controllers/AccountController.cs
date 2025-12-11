using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly ILogger<AccountController> _logger;

        private readonly ISmsService _smsService; // Додаємо SMS серві
        private readonly IMemoryCache _cache; // Для зберігання тимчасових кодів

        public AccountController(AgencyDBContext agencyDBContext, 
            IEmailService emailService, ILogger<AccountController> logger, 
            ISmsService smsService,
            IMemoryCache cache)
        {
            _agencyDBContext = agencyDBContext;
            _userModel = new UserModel(_agencyDBContext);
            _emailService = emailService;
            _logger = logger;

            _smsService = smsService;
            _cache = cache;
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


        //[HttpPost]
        //public async Task<IActionResult> CheckUser(string email, string password)
        //{
        //    try
        //    {
        //        User? user = _userModel.GetUserByEmail(email);

        //        if (user != null && SecurePasswordHasher.Verify(password, user.PasswordHash))
        //        {
        //            var claims = new List<Claim>
        //    {
        //        new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
        //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //        new Claim("UserId", user.Id.ToString()),
        //        new Claim("Login", user.Login)
        //    };

        //            var identity = new ClaimsIdentity(claims,
        //                CookieAuthenticationDefaults.AuthenticationScheme,
        //                ClaimsIdentity.DefaultNameClaimType,
        //                ClaimsIdentity.DefaultRoleClaimType);

        //            await HttpContext.SignInAsync(
        //                CookieAuthenticationDefaults.AuthenticationScheme,
        //                new ClaimsPrincipal(identity),
        //                new AuthenticationProperties
        //                {
        //                    IsPersistent = true,
        //                    ExpiresUtc = DateTime.UtcNow.AddDays(30)
        //                });

        //            TempData["LoginSuccess"] = $"Вітаємо, {user.Login}! Ви успішно увійшли в систему.";

        //            if (IsAdmin(user))
        //            {
        //                return RedirectToAction("Index", "Admin");
        //            }
        //            else
        //            {
        //                return RedirectToAction("Index", "Home");
        //            }
        //        }
        //        else
        //        {
        //            ViewBag.ErrorMessage = "Невірний email або пароль.";
        //            return View("LoginIn", new ErrorViewModel() { ErrorMessage = "User or Password incorrect" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "CheckUser error");
        //        ViewBag.ErrorMessage = "Сталася помилка при вході.";
        //        return View("LoginIn");
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> RegisterUser(string email, string login, string password,
    string confirmPassword, string? phoneNumber = null)
        {
            try
            {
                _logger.LogInformation($"=== REGISTER USER START ===");
                _logger.LogInformation($"Email: {email}, Login: {login}, Phone: {phoneNumber}");

                // 1. Базові перевірки
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(login) ||
                    string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    return View("RegisterIn", new ErrorViewModel()
                    {
                        ErrorMessage = "Всі обов'язкові поля мають бути заповнені"
                    });
                }

                email = email.Trim();
                login = login.Trim();

                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    phoneNumber = FormatPhoneNumber(phoneNumber);
                }

                // 2. Перевірка формату email
                if (!IsValidEmail(email))
                {
                    return View("RegisterIn", new ErrorViewModel()
                    {
                        ErrorMessage = "Некоректний формат Email"
                    });
                }

                // 3. Перевірка співпадіння паролів
                if (password != confirmPassword)
                {
                    return View("RegisterIn", new ErrorViewModel()
                    {
                        ErrorMessage = "Паролі не співпадають"
                    });
                }

                // 4. Перевірка мінімальної довжини пароля
                if (password.Length < 6)
                {
                    return View("RegisterIn", new ErrorViewModel()
                    {
                        ErrorMessage = "Пароль має містити мінімум 6 символів"
                    });
                }

                // 5. ДОДАТКОВА перевірка: якщо вказано телефон - перевіряємо формат
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    if (phoneNumber.Length < 10)
                    {
                        return View("RegisterIn", new ErrorViewModel()
                        {
                            ErrorMessage = "Номер телефону занадто короткий"
                        });
                    }
                }

                // 6. КОМПЛЕКСНА перевірка унікальності
                var existingUser = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u =>
                        u.Email == email ||
                        u.Login == login ||
                        (!string.IsNullOrWhiteSpace(phoneNumber) && u.PhoneNumber == phoneNumber));

                if (existingUser != null)
                {
                    _logger.LogInformation($"Existing user found: ID={existingUser.Id}, Email={existingUser.Email}, " +
                                         $"GoogleId={existingUser.GoogleId}, Phone={existingUser.PhoneNumber}");

                    // 🔴 ДОДАЙТЕ ЦЕЙ БЛОК ДЛЯ ТЕЛЕФОННИХ АКАУНТІВ ПЕРШИМ:
                    // 1. Якщо це телефонний акаунт (без GoogleId)
                    if (string.IsNullOrEmpty(existingUser.GoogleId) &&
                        !string.IsNullOrEmpty(existingUser.PhoneNumber) &&
                        existingUser.PhoneNumberConfirmed)
                    {
                        _logger.LogInformation($"Phone account found - redirecting to setup password");

                        // 🔴 ЗАПИСУЄМО В COOKIES (замість Session)
                        var cookieOptions = new CookieOptions
                        {
                            Expires = DateTime.Now.AddMinutes(10), // На 10 хвилин
                            HttpOnly = true,
                            Secure = true, // Якщо використовуєте HTTPS
                            SameSite = SameSiteMode.Lax
                        };

                        Response.Cookies.Append("PhoneAccount_UserId", existingUser.Id.ToString(), cookieOptions);
                        Response.Cookies.Append("PhoneAccount_Email", existingUser.Email, cookieOptions);
                        Response.Cookies.Append("PhoneAccount_Phone", existingUser.PhoneNumber, cookieOptions);
                        Response.Cookies.Append("PhoneAccount_Login", existingUser.Login, cookieOptions);

                        // Перенаправляємо на спеціальну сторінку
                        return RedirectToAction("SetupPasswordForPhone", new
                        {
                            userId = existingUser.Id,
                            email = existingUser.Email,
                            phone = existingUser.PhoneNumber,
                            login = existingUser.Login
                        });
                    }

                    // === КІНЕЦЬ ДОДАВАНОГО БЛОКУ ===





                    // Визначаємо, що саме співпало
                    bool emailMatch = existingUser.Email == email;
                    bool loginMatch = existingUser.Login == login;
                    bool phoneMatch = !string.IsNullOrWhiteSpace(phoneNumber) &&
                                    existingUser.PhoneNumber == phoneNumber;

                    // 6.1. GOOGLE-АКАУНТ - пропонуємо прив'язати пароль
                    if (!string.IsNullOrEmpty(existingUser.GoogleId))
                    {
                        if (emailMatch)
                        {
                            TempData["LinkAccountEmail"] = email;
                            TempData["LinkAccountLogin"] = login;

                            // Якщо це також телефонний акаунт - пропонуємо об'єднати
                            if (phoneMatch)
                            {
                                TempData["ErrorMessage"] = "Цей email вже зареєстровано через Google, " +
                                                          "а цей номер телефону також зареєстровано. " +
                                                          "Будь ласка, увійдіть через Google.";
                                return RedirectToAction("LinkPasswordToGoogle");
                            }

                            TempData["SuccessMessage"] = "Цей email вже зареєстровано через Google. " +
                                                        "Ви можете прив'язати пароль для входу через email.";
                            return RedirectToAction("LinkPasswordToGoogle");
                        }
                    }

                    // 6.2. СПРОБА РЕЄСТРАЦІЇ З ТЕЛЕФОНОМ, ЯКИЙ ВЖЕ ІСНУЄ
                    if (phoneMatch)
                    {
                        // Телефон вже зареєстрований
                        return View("RegisterIn", new ErrorViewModel()
                        {
                            ErrorMessage = "Користувач з таким номером телефону вже існує. " +
                                          "Спробуйте увійти через телефон."
                        });
                    }

                    // 6.3. EMAIL ВЖЕ ІСНУЄ (не Google)
                    if (emailMatch)
                    {
                        return View("RegisterIn", new ErrorViewModel()
                        {
                            ErrorMessage = "Користувач з таким email вже існує. " +
                                          "Спробуйте увійти або використати інший email."
                        });
                    }

                    // 6.4. LOGIN ВЖЕ ІСНУЄ
                    if (loginMatch)
                    {
                        return View("RegisterIn", new ErrorViewModel()
                        {
                            ErrorMessage = "Цей логін вже зайнятий. Будь ласка, виберіть інший логін."
                        });
                    }

                    // Запасний варіант
                    return View("RegisterIn", new ErrorViewModel()
                    {
                        ErrorMessage = "Користувач з такими даними вже існує"
                    });
                }

                // 7. Перевірка, чи можливо це телефонний акаунт без email?
                // Якщо email вже є у іншого користувача як додатковий - не дозволяємо
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    var userWithSamePhone = await _agencyDBContext.Users
                        .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

                    if (userWithSamePhone != null)
                    {
                        return View("RegisterIn", new ErrorViewModel()
                        {
                            ErrorMessage = "Цей номер телефону вже використовується іншим користувачем"
                        });
                    }
                }

                // 8. Створення нового користувача
                var newUser = new User
                {
                    Email = email,
                    Login = login,
                    PasswordHash = SecurePasswordHasher.Hash(password),
                    DateOfCreat = DateTime.Now,
                    DateOfUpdated = null
                };

                // Додаємо телефон, якщо вказано
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    newUser.PhoneNumber = phoneNumber;
                    newUser.PhoneNumberConfirmed = false; // Потребує підтвердження
                }

                _agencyDBContext.Users.Add(newUser);
                await _agencyDBContext.SaveChangesAsync();

                _logger.LogInformation($"✅ New user created: ID={newUser.Id}, Email={newUser.Email}");

                // 9. Автоматичний вхід після реєстрації
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
            new Claim(ClaimTypes.Email, newUser.Email),
            new Claim(ClaimTypes.Name, newUser.Login),
            new Claim("UserId", newUser.Id.ToString()),
            new Claim("Login", newUser.Login)
        };

                // Додаємо claim для телефону, якщо є
                if (!string.IsNullOrWhiteSpace(newUser.PhoneNumber))
                {
                    claims.Add(new Claim(ClaimTypes.MobilePhone, newUser.PhoneNumber));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30)
                    });

                _logger.LogInformation($"✅ User {newUser.Email} logged in after registration");

                // 10. Відправляємо SMS для підтвердження телефону, якщо потрібно
                if (!string.IsNullOrWhiteSpace(phoneNumber) && !newUser.PhoneNumberConfirmed)
                {
                    try
                    {
                        var code = GenerateVerificationCode();
                        var cacheKey = $"PhoneVerify_{phoneNumber}";
                        _cache.Set(cacheKey, code, TimeSpan.FromMinutes(10));

                        await _smsService.SendSmsAsync(phoneNumber,
                            $"Ваш код підтвердження: {code}");

                        TempData["PhoneVerificationRequired"] = "true";
                        TempData["PhoneNumber"] = phoneNumber;

                        _logger.LogInformation($"📱 Verification code sent to: {phoneNumber}");
                    }
                    catch (Exception smsEx)
                    {
                        _logger.LogError(smsEx, "Failed to send SMS verification");
                        // Не блокуємо реєстрацію через помилку SMS
                    }
                }

                // 11. Повідомлення про успіх
                TempData["SuccessMessage"] = "Реєстрація пройшла успішно! Ласкаво просимо!";
                TempData["RegisteredUserName"] = newUser.Login;
                TempData["RegisteredUserEmail"] = newUser.Email;

                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    TempData["RegisteredUserPhone"] = phoneNumber;
                }

                _logger.LogInformation($"=== REGISTER USER END ===");

                // 12. Редирект
                // Якщо потрібно підтвердити телефон - на сторінку верифікації
                if (TempData.ContainsKey("PhoneVerificationRequired"))
                {
                    return RedirectToAction("VerifyPhone");
                }

                return RedirectToAction("RegistrationSuccess");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error in RegisterUser");
                return View("RegisterIn", new ErrorViewModel()
                {
                    ErrorMessage = "Помилка бази даних при реєстрації. Спробуйте ще раз."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при реєстрації");
                return View("RegisterIn", new ErrorViewModel()
                {
                    ErrorMessage = $"Помилка при реєстрації: {ex.Message}"
                });
            }
        }

  
        [HttpGet]
        public IActionResult SetupPasswordForPhone(int? userId, string? email, string? phone, string? login)
        {
            // Перевірка через query parameters (пріоритет)
            if (userId != null && !string.IsNullOrEmpty(email))
            {
                ViewBag.ExistingUserId = userId;
                ViewBag.ExistingEmail = email;
                ViewBag.ExistingPhone = phone;
                ViewBag.ExistingLogin = login;
                return View("SetupPassword");
            }

            // 🔴 СПРОБУВАТИ ОТРИМАТИ З COOKIES
            var cookieUserId = Request.Cookies["PhoneAccount_UserId"];
            var cookieEmail = Request.Cookies["PhoneAccount_Email"];
            var cookiePhone = Request.Cookies["PhoneAccount_Phone"];
            var cookieLogin = Request.Cookies["PhoneAccount_Login"];

            if (!string.IsNullOrEmpty(cookieUserId))
            {
                ViewBag.ExistingUserId = int.Parse(cookieUserId);
                ViewBag.ExistingEmail = cookieEmail;
                ViewBag.ExistingPhone = cookiePhone;
                ViewBag.ExistingLogin = cookieLogin;
                return View("SetupPassword");
            }

            // Якщо нічого не знайдено
            TempData["ErrorMessage"] = "Сесія закінчилася. Спробуйте знову.";
            return RedirectToAction("RegisterIn");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupPasswordForPhone(int userId, string email, string password,
    string confirmPassword, string? phone = null, string? login = null)
        {
            try
            {
                _logger.LogInformation($"=== SETUP PASSWORD FOR PHONE ACCOUNT POST ===");
                _logger.LogInformation($"UserId: {userId}, Email: {email}");

                // Перевірка вхідних даних
                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    TempData["ErrorMessage"] = "Пароль має містити мінімум 6 символів";

                    // 🔴 ЗАЛИШАЄМО ViewBag для відображення на цій же сторінці
                    ViewBag.ExistingUserId = userId;
                    ViewBag.ExistingEmail = email;
                    ViewBag.ExistingPhone = phone;
                    ViewBag.ExistingLogin = login;

                    // 🔴 Повертаємо View (не Redirect)
                    return View("SetupPassword");
                }

                if (password != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Паролі не співпадають";

                    // 🔴 ЗАЛИШАЄМО ViewBag для відображення на цій же сторінці
                    ViewBag.ExistingUserId = userId;
                    ViewBag.ExistingEmail = email;
                    ViewBag.ExistingPhone = phone;
                    ViewBag.ExistingLogin = login;

                    // 🔴 Повертаємо View (не Redirect)
                    return View("SetupPassword");
                }

                // Знаходимо користувача
                var user = await _agencyDBContext.Users.FindAsync(userId);
                if (user == null || user.Email != email)
                {
                    TempData["ErrorMessage"] = "Користувача не знайдено";
                    return RedirectToAction("RegisterIn");
                }

                // Перевіряємо, що це телефонний акаунт
                if (string.IsNullOrEmpty(user.PhoneNumber) || !user.PhoneNumberConfirmed)
                {
                    TempData["ErrorMessage"] = "Це не телефонний акаунт";
                    return RedirectToAction("LoginIn");
                }

                // Оновлюємо пароль
                user.PasswordHash = SecurePasswordHasher.Hash(password);

                // Оновлюємо логін, якщо потрібно
                if (!string.IsNullOrWhiteSpace(login) && user.Login != login)
                {
                    user.Login = login;
                }

                user.DateOfUpdated = DateTime.Now;

                await _agencyDBContext.SaveChangesAsync();

                _logger.LogInformation($"✅ Password set for phone account: ID={user.Id}");

                // Автоматично входимо
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("UserId", user.Id.ToString()),
            new Claim("Login", user.Login),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? "")
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30)
                    });

                // 🔴 ОЧИЩАЄМО COOKIES ПІСЛЯ УСПІШНОЇ ОПЕРАЦІЇ
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                };

                Response.Cookies.Append("PhoneAccount_UserId", "", cookieOptions);
                Response.Cookies.Append("PhoneAccount_Email", "", cookieOptions);
                Response.Cookies.Append("PhoneAccount_Phone", "", cookieOptions);
                Response.Cookies.Append("PhoneAccount_Login", "", cookieOptions);

                TempData["SuccessMessage"] = "Пароль успішно встановлено! Ви увійшли в систему.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetupPasswordForPhone error");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;

                // 🔴 ЗАЛИШАЄМО ViewBag для відображення на цій же сторінці
                ViewBag.ExistingUserId = userId;
                ViewBag.ExistingEmail = email;
                ViewBag.ExistingPhone = phone;
                ViewBag.ExistingLogin = login;

                // 🔴 Повертаємо View (не Redirect)
                return View("SetupPassword");
            }
        }




        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Перевірка за допомогою регулярного виразу
                var emailRegex = new System.Text.RegularExpressions.Regex(
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }




        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();
            TempData["LogoutSuccess"] = "Ви успішно вийшли з акаунту";
            return RedirectToAction("LoginIn");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Очищаємо важливі cookies
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1),
                HttpOnly = true,
                Path = "/"
            };

            Response.Cookies.Append("LinkPhone_Email", "", cookieOptions);
            Response.Cookies.Append("LinkPhone_Phone", "", cookieOptions);
            Response.Cookies.Append("PhoneAccount_UserId", "", cookieOptions);

            TempData["LogoutSuccess"] = "Ви успішно вийшли з акаунту";
            return RedirectToAction("Index", "Home");
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


                    // ========== ПОПРАВЛЕНА ПЕРЕВІРКА ДЛЯ GOOGLE-КОРИСТУВАЧІВ ==========
                    if (!string.IsNullOrEmpty(user.GoogleId))
                    {
                        // Google-користувач БЕЗ власного пароля
                        if (string.IsNullOrEmpty(user.PasswordHash))
                        {
                            TempData["ErrorMessage"] = "Цей акаунт зареєстровано через Google і не має власного пароля. " +
                                                      "Для входу використовуйте кнопку 'Увійти через Google'.";
                            return View(model);
                        }
                        // Google-користувач З власним паролем
                        else
                        {
                            // ПРОПУСКАЄМО - він може відновити пароль!
                            _logger.LogInformation($"Google user with custom password: {user.Email}");
                        }
                    }
                    // ========== КІНЕЦЬ ПЕРЕВІРКИ ==========



                    var resetCode = new Random().Next(100000, 999999).ToString();

                    // Зберігаємо код
                    user.ResetPasswordCode = resetCode;
                    user.ResetPasswordCodeExpires = DateTime.UtcNow.AddMinutes(15);
                    user.ResetPasswordCodeUsed = false;

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

                if (user != null)
                {
                    // ========== ПОПРАВЛЕНА ПЕРЕВІРКА: Google-користувач ==========
                    if (!string.IsNullOrEmpty(user.GoogleId))
                    {
                        // Google-користувач БЕЗ власного пароля
                        if (string.IsNullOrEmpty(user.PasswordHash))
                        {
                            ModelState.AddModelError("", "Цей акаунт зареєстровано через Google і не має власного пароля. " +
                                                        "Використовуйте кнопку 'Увійти через Google'.");
                            return View(model);
                        }
                        // Google-користувач З власним паролем
                        else
                        {
                            // ПРОПУСКАЄМО - він може змінити пароль!
                            _logger.LogInformation($"Resetting password for Google user with custom password: {user.Email}");
                        }
                    }
                    // ========== КІНЕЦЬ ПЕРЕВІРКИ ==========



                    if (user.ResetPasswordCode == model.Code &&
                        user.ResetPasswordCodeExpires > DateTime.UtcNow &&
                        !user.ResetPasswordCodeUsed)
                    {
                        // Код вірний і не прострочений - змінюємо пароль
                        user.PasswordHash = SecurePasswordHasher.Hash(model.Password);
                        user.ResetPasswordCodeUsed = true;
                        user.ResetPasswordCode = null;
                        user.ResetPasswordCodeExpires = null;


                       // ✅ ОНОВЛЮЄМО ДАТУ ПРИ ЗМІНІ ПАРОЛЯ
                        user.DateOfUpdated = DateTime.Now;

                        _agencyDBContext.Users.Update(user);
                        await _agencyDBContext.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Пароль успішно змінено! Тепер ви можете увійти з новим паролем.";
                        return RedirectToAction("LoginIn");
                    }
                    else
                    {
                        ModelState.AddModelError("Code", "Невірний або прострочений код підтвердження");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Користувача не знайдено");
                }
            }

            return View(model);
        }

        private string GenerateResetCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // 6-значний код
        }



        // Додайте цей метод у клас AccountController
        private string GetAccountType(User user)
        {
            if (!string.IsNullOrEmpty(user.GoogleId))
            {
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    return "Google-акаунт з власним паролем";
                }
                else
                {
                    return "Google-акаунт без пароля";
                }
            }
            else if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumberConfirmed)
            {
                return "Телефонний акаунт";
            }
            else
            {
                return "Звичайний email-акаунт";
            }
        }


        //private bool IsAdmin(User user)
        //{
        //    return user.Email == "admin@admin.com";
        //}






        [HttpPost]
        public async Task<IActionResult> LoginIn(string email, string password, string? returnUrl = null)
        {
            try
            {
                _logger.LogInformation($"=== LOGIN IN START ===");
                _logger.LogInformation($"Attempting login with: {email}");

                // 1. Перевірка вхідних даних
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.ErrorMessage = "Будь ласка, заповніть всі поля";
                    return View("LoginIn", new ErrorViewModel() { ErrorMessage = "Заповніть всі поля" });
                }

                User? user = null;

                // 2. Пошук користувача за email
                user = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                // 3. Якщо не знайдено за email, спробувати за телефоном
                if (user == null)
                {
                    if (email.Contains("+") || email.All(char.IsDigit))
                    {
                        var formattedPhone = FormatPhoneNumber(email);
                        user = await _agencyDBContext.Users
                            .FirstOrDefaultAsync(u => u.PhoneNumber == formattedPhone && u.PhoneNumberConfirmed);
                    }
                }

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Невірний email/телефон або пароль.";
                    return View("LoginIn", new ErrorViewModel() { ErrorMessage = "User or Password incorrect" });
                }

                _logger.LogInformation($"User found: ID={user.Id}, Email={user.Email}, GoogleId={user.GoogleId}");

                // 4. Спеціальна обробка Google-користувачів
                if (!string.IsNullOrEmpty(user.GoogleId))
                {
                    // Google-користувач може не мати пароля
                    if (string.IsNullOrEmpty(user.PasswordHash))
                    {
                        TempData["LinkAccountEmail"] = user.Email;
                        TempData["LinkAccountLogin"] = user.Login;
                        TempData["ErrorMessage"] = "Цей акаунт зареєстровано через Google. " +
                                                  "Увійдіть через Google або встановіть пароль.";
                        return RedirectToAction("LinkPasswordToGoogle");
                    }
                }

                // 5. Перевірка пароля
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    ViewBag.ErrorMessage = "Для цього акаунта не встановлено пароль.";
                    return View("LoginIn");
                }

                if (!SecurePasswordHasher.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning($"Invalid password for: {user.Email}");
                    ViewBag.ErrorMessage = "Невірний пароль.";

                    // Спеціальне повідомлення для Google-користувачів
                    if (!string.IsNullOrEmpty(user.GoogleId))
                    {
                        ViewBag.ErrorMessage += " Це Google-акаунт. Спробуйте увійти через Google або відновіть пароль.";
                    }

                    return View("LoginIn", new ErrorViewModel() { ErrorMessage = "User or Password incorrect" });
                }

                // 6. 🔴 ПЕРЕВІРКА: Чи є cookies для прив'язування телефону?
                var linkPhoneEmail = Request.Cookies["LinkPhone_Email"];
                var linkPhonePhone = Request.Cookies["LinkPhone_Phone"];

                _logger.LogInformation($"Checking phone link cookies: Email={linkPhoneEmail}, Phone={linkPhonePhone}");

                if (!string.IsNullOrEmpty(linkPhoneEmail) && linkPhoneEmail == user.Email)
                {
                    _logger.LogInformation($"Found phone link cookie for this user. Phone to link: {linkPhonePhone}");

                    // Прив'язуємо телефон до цього акаунта
                    if (!string.IsNullOrEmpty(linkPhonePhone))
                    {
                        // Перевіряємо, чи телефон не зайнятий іншим користувачем
                        var phoneUsedByOther = await _agencyDBContext.Users
                            .AnyAsync(u => u.PhoneNumber == linkPhonePhone && u.Id != user.Id);

                        if (!phoneUsedByOther)
                        {
                            user.PhoneNumber = linkPhonePhone;
                            user.PhoneNumberConfirmed = true;
                            user.DateOfUpdated = DateTime.Now;

                            await _agencyDBContext.SaveChangesAsync();

                            _logger.LogInformation($"✅ Phone {linkPhonePhone} attached to user {user.Email}");

                            // Очищаємо cookies
                            var cookieOptions = new CookieOptions
                            {
                                Expires = DateTime.Now.AddDays(-1),
                                HttpOnly = true,
                                Secure = false,
                                SameSite = SameSiteMode.Lax,
                                Path = "/"
                            };

                            Response.Cookies.Append("LinkPhone_Email", "", cookieOptions);
                            Response.Cookies.Append("LinkPhone_Phone", "", cookieOptions);
                            Response.Cookies.Append("LinkPhone_Login", "", cookieOptions);

                            TempData["SuccessMessage"] = $"Телефон {linkPhonePhone} успішно прив'язано до вашого акаунта!";
                        }
                    }
                }

                // 7. Створення claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Login ?? user.Email),
            new Claim("UserId", user.Id.ToString()),
            new Claim("Login", user.Login ?? user.Email)
        };

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
                    claims.Add(new Claim("PhoneConfirmed", user.PhoneNumberConfirmed.ToString()));
                }

                if (!string.IsNullOrEmpty(user.GoogleId))
                {
                    claims.Add(new Claim("GoogleId", user.GoogleId));
                }

                if (IsAdmin(user))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }

                // 8. Авторизація
                var identity = new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    ClaimTypes.Name,
                    ClaimTypes.Role);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30),
                        AllowRefresh = true
                    });

                _logger.LogInformation($"✅ User {user.Email} successfully logged in");

                // 9. Повідомлення про успіх
                TempData["LoginSuccess"] = $"Вітаємо, {user.Login ?? user.Email}! Ви успішно увійшли.";

                // 10. Редирект
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                if (IsAdmin(user))
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginIn error");
                ViewBag.ErrorMessage = "Сталася помилка при вході.";
                return View("LoginIn");
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            try
            {
                // Перевіряємо чи є ClientId
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var clientId = configuration["Authentication:Google:ClientId"];

                if (string.IsNullOrEmpty(clientId) || clientId.Contains("test-client-id"))
                {
                    _logger.LogWarning("Google ClientId not configured properly");
                    TempData["ErrorMessage"] = "Google авторизація не налаштована. Використовується тестовий режим.";
                    return RedirectToAction("TestGoogleAuth");
                }

                _logger.LogInformation("Starting REAL Google authentication...");

                var properties = new AuthenticationProperties
                {
                    RedirectUri = returnUrl ?? "/Account/GoogleResponse"
                };

                return Challenge(properties, GoogleDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GoogleLogin");
                TempData["ErrorMessage"] = $"Помилка: {ex.Message}";
                return RedirectToAction("LoginIn");
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                _logger.LogInformation("=== GOOGLE RESPONSE START ===");

                // 1. Отримати результат від Google
                var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

                if (!result?.Succeeded ?? true)
                {
                    _logger.LogError("Google authentication failed");
                    TempData["ErrorMessage"] = "Google authentication failed";
                    return RedirectToAction("LoginIn");
                }

                // 2. Отримати дані
                var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
                var googleId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                _logger.LogInformation($"Received from Google - Email: {email}, Name: {name}, GoogleId: {googleId}");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("No email received from Google");
                    TempData["ErrorMessage"] = "No email received from Google";
                    return RedirectToAction("LoginIn");
                }

                // 3. Знайти або створити користувача
                var user = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                bool isNewUser = false;

                if (user == null)
                {
                    // 🔴 Це НОВИЙ користувач (перша реєстрація через Google)
                    isNewUser = true;

                    user = new User
                    {
                        Email = email,
                        Login = name ?? email.Split('@')[0],
                        PasswordHash = SecurePasswordHasher.Hash(Guid.NewGuid().ToString()),
                        DateOfCreat = DateTime.Now,
                        GoogleId = googleId
                    };

                    _agencyDBContext.Users.Add(user);
                    await _agencyDBContext.SaveChangesAsync();

                    _logger.LogInformation($"✅ New Google user created: {email}");

                    // 🔴 Встановлюємо TempData для відображення повідомлення про новий акаунт
                    TempData["IsNewGoogleUser"] = true;
                }
                else
                {
                    // 🔴 Це існуючий користувач - ОБ'ЄДНУЄМО!

                    // А) Додаємо GoogleId, якщо його немає
                    if (string.IsNullOrEmpty(user.GoogleId) && !string.IsNullOrEmpty(googleId))
                    {
                        user.GoogleId = googleId;
                        _logger.LogInformation($"✅ Added GoogleId to existing user: {email}");
                    }

                    // Б) Оновлюємо ім'я, якщо потрібно
                    if (string.IsNullOrEmpty(user.Login) && !string.IsNullOrEmpty(name))
                    {
                        user.Login = name;
                    }

                    // В) Оновлюємо дату
                    user.DateOfUpdated = DateTime.Now;

                    await _agencyDBContext.SaveChangesAsync();
                    _logger.LogInformation($"✅ Updated existing user: {email}");

                    // 🔴 Встановлюємо TempData для відображення повідомлення про вхід
                    TempData["IsExistingUser"] = true;
                }

                // 4. Створити claims для нашої системи
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("UserId", user.Id.ToString()),
            new Claim("Login", user.Login),
            new Claim("IsGoogleUser", (!string.IsNullOrEmpty(user.GoogleId)).ToString())
        };

                if (IsAdmin(user))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }

                // 5. Створити identity
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Видалити тимчасові куки Google (якщо є)
                if (HttpContext.Request.Cookies.ContainsKey(".AspNetCore.Google"))
                {
                    HttpContext.Response.Cookies.Delete(".AspNetCore.Google");
                }

                // 6. ВХІД в нашу систему
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30)
                    });

                _logger.LogInformation($"✅ User signed in: {user.Email}");

                // 7. Редирект з відповідним повідомленням
                if (isNewUser)
                {
                    // 🔴 НОВИЙ користувач - показуємо Welcome сторінку
                    TempData["LoginSuccess"] = $"Вітаємо, {user.Login}! Ви успішно зареєструвалися через Google.";
                    return RedirectToAction("Welcome", "Account");
                }
                else
                {
                    // 🔴 ІСНУЮЧИЙ користувач - перенаправляємо на Home
                    TempData["LoginSuccess"] = $"Вітаємо, {user.Login}! Ви успішно увійшли через Google.";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GoogleResponse error");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("LoginIn");
            }
        }














        [HttpPost("reset-google-password")]
        public async Task<IActionResult> ResetGooglePassword(string email)
        {
            var user = await _agencyDBContext.Users
                .FirstOrDefaultAsync(u => u.Email == email && !string.IsNullOrEmpty(u.GoogleId));

            if (user == null)
            {
                return BadRequest("Користувач не знайдений або не Google-користувач");
            }

            // Генеруємо новий випадковий пароль (для внутрішніх потреб)
            var newPassword = Guid.NewGuid().ToString();
            user.PasswordHash = SecurePasswordHasher.Hash(newPassword);
            user.DateOfUpdated = DateTime.Now;

            await _agencyDBContext.SaveChangesAsync();

            _logger.LogInformation($"Password reset for Google user: {email}");

            return Ok($"Новий пароль згенеровано (лише для адміністративних потреб)");
        }













        //[HttpGet]
        //public IActionResult Welcome()
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("LoginIn");
        //    }

        //    ViewBag.UserName = User.Identity.Name;
        //    ViewBag.Email = User.FindFirst(ClaimTypes.Email)?.Value;
        //    ViewBag.IsGoogleUser = !string.IsNullOrEmpty(User.FindFirst("GoogleId")?.Value);

        //    return View();
        //}

        [HttpGet]
        public IActionResult Welcome()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginIn");
            }

            var userId = User.FindFirst("UserId")?.Value;

            // Знаходимо користувача в базі для отримання повної інформації
            var user = _agencyDBContext.Users.Find(int.Parse(userId));

            ViewBag.UserName = User.Identity.Name;
            ViewBag.Email = User.FindFirst(ClaimTypes.Email)?.Value;

            // Визначаємо тип акаунта
            ViewBag.AccountType = GetWelcomeAccountType(user);

            return View();
        }

        private string GetWelcomeAccountType(User user)
        {
            if (user == null) return "Невідомий";

            // 1. Google-акаунт
            if (!string.IsNullOrEmpty(user.GoogleId))
            {
                return "Google";
            }

            // 2. Телефонний акаунт (перевіряємо, що телефон підтверджений і це основний спосіб)
            if (!string.IsNullOrEmpty(user.PhoneNumber) &&
                user.PhoneNumberConfirmed &&
                string.IsNullOrEmpty(user.PasswordHash)) // Якщо немає пароля - це чистий телефонний акаунт
            {
                return "Телефон";
            }

            // 3. Email-акаунт (з паролем)
            if (!string.IsNullOrEmpty(user.PasswordHash) &&
                string.IsNullOrEmpty(user.GoogleId) &&
                (string.IsNullOrEmpty(user.PhoneNumber) || !user.PhoneNumberConfirmed))
            {
                return "Email";
            }

            // 4. Гібридний акаунт (телефон + email)
            if (!string.IsNullOrEmpty(user.PhoneNumber) &&
                user.PhoneNumberConfirmed &&
                !string.IsNullOrEmpty(user.PasswordHash))
            {
                return "Email та Телефон";
            }

            return "Комбінований";
        }




        // ========== ДОПОМІЖНІ МЕТОДИ ДЛЯ ТЕЛЕФОНІВ ==========

        private async Task SendPhoneVerificationCode(User user)
        {
            var code = GenerateVerificationCode();
            var cacheKey = $"PhoneVerify_{user.PhoneNumber}";

            // Зберігаємо код на 10 хвилин
            _cache.Set(cacheKey, code, TimeSpan.FromMinutes(10));

            // Відправляємо SMS
            await _smsService.SendSmsAsync(user.PhoneNumber,
                $"Ваш код підтвердження: {code}");
        }

        private async Task SendPhoneLoginCode(User user)
        {
            try
            {
                var code = GenerateVerificationCode();
                var cacheKey = $"PhoneLogin_{user.PhoneNumber}";

                // Зберігаємо код на 5 хвилин
                _cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));

                // Відправляємо SMS
                await _smsService.SendSmsAsync(user.PhoneNumber,
                    $"Код для входу: {code}. Дійсний 5 хвилин.");

                _logger.LogInformation($"SMS sent to {user.PhoneNumber}: {code}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS");
                throw; // або обробіть помилку по-іншому
            }
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string FormatPhoneNumber(string phone)
        {
            phone = phone.Trim().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (phone.StartsWith("0"))
                phone = "+38" + phone;
            else if (phone.StartsWith("80"))
                phone = "+3" + phone;
            else if (!phone.StartsWith("+"))
                phone = "+" + phone;

            return phone;
        }

        private AuthType GetAuthType(User user)
        {
            if (!string.IsNullOrEmpty(user.GoogleId))
                return AuthType.Google;

            if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumberConfirmed)
                return AuthType.Phone;

            return AuthType.EmailPassword;
        }






        // 1. Сторінка для введення номера телефону (GET)
        [HttpGet]
        public IActionResult LoginWithPhone()
        {

            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            TempData.Remove("InfoMessage");
            return View();
        }

        // 2. Обробка POST запиту (спрощений тестовий варіант)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithPhone(string phoneNumber)
        {
            try
            {
                _logger.LogInformation($"=== LOGIN WITH PHONE POST ===");
                _logger.LogInformation($"Request login for phone: {phoneNumber}");

                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    TempData["ErrorMessage"] = "Будь ласка, введіть номер телефону";
                    return View();
                }

                var formattedPhone = FormatPhoneNumber(phoneNumber);

                // 🔴 ВАЖЛИВО: Шукаємо користувача за телефоном
                var user = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == formattedPhone && u.PhoneNumberConfirmed);

                if (user == null)
                {
                    // Користувача не знайдено - пропонуємо реєстрацію
                    TempData["ErrorMessage"] = "Користувача з цим номером телефону не знайдено. Зареєструйтесь спочатку.";
                    TempData["PhoneNumber"] = formattedPhone;
                    return RedirectToAction("RegisterWithPhone", new { phone = formattedPhone });
                }

                // 🔴 Генеруємо код для входу
                var code = GenerateVerificationCode();
                var cacheKey = $"PhoneLogin_{formattedPhone}";

                // Зберігаємо код І інформацію про користувача
                _cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));

                // Додатково зберігаємо ID користувача для подальшої авторизації
                _cache.Set($"PhoneUser_{formattedPhone}", user.Id, TimeSpan.FromMinutes(5));

                _logger.LogInformation($"Login code generated: {code} for user: {user.Email}");

                // 🔴 Відправляємо SMS (або тестове повідомлення)
                try
                {
                    // Якщо SMS сервіс працює
                    await _smsService.SendSmsAsync(formattedPhone,
                        $"Код для входу: {code}. Дійсний 5 хвилин.");
                    TempData["SuccessMessage"] = "Код відправлено на ваш телефон";
                }
                catch (Exception smsEx)
                {
                    // Для тесту - показуємо код на екрані
                    _logger.LogError(smsEx, "Failed to send SMS");
                    TempData["SuccessMessage"] = $"Тестовий код: {code}";
                }

                // Зберігаємо номер телефону для наступної сторінки
                TempData["PhoneNumber"] = formattedPhone;

                // Перенаправляємо на сторінку верифікації
                return RedirectToAction("VerifyPhoneLogin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginWithPhone error");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                return View();
            }
        }

        // 3. Сторінка для введення коду (GET)
        [HttpGet]
        public IActionResult VerifyPhoneLogin()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneLogin(string phoneNumber, string code)
        {
            try
            {
                _logger.LogInformation($"=== VERIFY PHONE LOGIN POST ===");
                _logger.LogInformation($"Verifying code {code} for phone {phoneNumber}");

                var formattedPhone = FormatPhoneNumber(phoneNumber);
                var cacheKey = $"PhoneLogin_{formattedPhone}";

                // 1. Перевіряємо код
                if (!_cache.TryGetValue(cacheKey, out string storedCode) || storedCode != code)
                {
                    TempData["ErrorMessage"] = $"Невірний код або час вийшов. Очікував: {storedCode}, отримав: {code}";
                    TempData["PhoneNumber"] = phoneNumber;
                    return View();
                }

                // 2. Отримуємо ID користувача з кешу
                var userCacheKey = $"PhoneUser_{formattedPhone}";
                if (!_cache.TryGetValue(userCacheKey, out int userId))
                {
                    // Якщо немає в кешу, шукаємо в базі
                    var userFromDb = await _agencyDBContext.Users
                        .FirstOrDefaultAsync(u => u.PhoneNumber == formattedPhone && u.PhoneNumberConfirmed);

                    if (userFromDb == null)
                    {
                        TempData["ErrorMessage"] = "Користувача не знайдено";
                        TempData["PhoneNumber"] = phoneNumber;
                        return View();
                    }
                    userId = userFromDb.Id;
                }

                // 3. Знаходимо користувача
                var user = await _agencyDBContext.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Користувача не знайдено";
                    TempData["PhoneNumber"] = phoneNumber;
                    return View();
                }

                user.DateOfUpdated = DateTime.Now;
                await _agencyDBContext.SaveChangesAsync();
                _logger.LogInformation($"✅ Phone login verified for user: ID={user.Id}, Email={user.Email}");

                // 4. Авторизуємо користувача
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("UserId", user.Id.ToString()),
            new Claim("Login", user.Login),
            new Claim("PhoneNumber", user.PhoneNumber ?? "")
        };

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
                    claims.Add(new Claim("PhoneConfirmed", user.PhoneNumberConfirmed.ToString()));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30)
                    });

                // 5. Очищаємо кеш
                _cache.Remove(cacheKey);
                _cache.Remove(userCacheKey);

                _logger.LogInformation($"✅ User {user.Email} successfully logged in via phone");

                TempData["LoginSuccess"] = $"Вітаємо, {user.Login}! Ви успішно увійшли через телефон.";

                if (IsAdmin(user))
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyPhoneLogin error");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                TempData["PhoneNumber"] = phoneNumber;
                return View();
            }
        }





        // 2. Обробка введеного номера (відправка SMS)
        [HttpPost]
        public async Task<IActionResult> RequestPhoneLogin(string phoneNumber)
        {
            try
            {
                var formattedPhone = FormatPhoneNumber(phoneNumber);

                // Шукаємо користувача
                var user = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == formattedPhone && u.PhoneNumberConfirmed);

                if (user == null)
                {
                    // Новий користувач - пропонуємо реєстрацію
                    TempData["PhoneNumber"] = formattedPhone;
                    return RedirectToAction("RegisterWithPhone", new { phone = formattedPhone });
                }

                // Відправляємо код для входу
                await SendPhoneLoginCode(user); // ← ЦЕЙ МЕТОД ЗБЕРІГАЄ КОД

                TempData["SuccessMessage"] = "Код для входу відправлено на ваш телефон";
                TempData["PhoneNumber"] = formattedPhone;
                return RedirectToAction("VerifyPhoneLogin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при запиті входу через телефон");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                return View("LoginWithPhone");
            }
        }

       







        // ========== ТЕЛЕФОННА РЕЄСТРАЦІЯ ТА ВХІД ==========


        // 📱 Сторінка реєстрації через телефон (GET - показує форму)
        // GET: Сторінка для прив'язування телефону після входу
        [HttpGet]
        [HttpGet]
        public IActionResult LinkPhoneAfterLogin()
        {
            // 🔴 СПРОБУВАТИ ОТРИМАТИ З COOKIES
            var cookieEmail = Request.Cookies["LinkPhone_Email"];
            var cookiePhone = Request.Cookies["LinkPhone_Phone"];
            var cookieLogin = Request.Cookies["LinkPhone_Login"];

            // 🔴 ДОДАТКОВА ПЕРЕВІРКА: Логування для дебагу
            _logger.LogInformation($"LinkPhoneAfterLogin GET - Cookies: Email={cookieEmail}, " +
                $"Phone={cookiePhone}, Login={cookieLogin}");


            if (!string.IsNullOrEmpty(cookieEmail))
            {
                ViewBag.LinkPhoneEmail = cookieEmail;
                ViewBag.LinkPhonePhone = cookiePhone;
                ViewBag.LinkPhoneLogin = cookieLogin;
                return View();
            }

            //// Якщо немає cookies, спробувати TempData
            //if (TempData["LinkPhoneEmail"] == null)
            //{
            //    return RedirectToAction("LoginIn");
            //}

            //ViewBag.LinkPhoneEmail = TempData["LinkPhoneEmail"];
            //ViewBag.LinkPhonePhone = TempData["LinkPhonePhone"];
            //ViewBag.LinkPhoneLogin = TempData["LinkPhoneLogin"];

            return View();
        }

        // POST: Обробити прив'язування телефону (для авторизованих)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkPhoneAfterLogin(string confirm = "yes")
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var user = await _agencyDBContext.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Користувача не знайдено";
                    return RedirectToAction("LoginIn");
                }

                // Отримуємо дані з TempData
                var phoneToLink = TempData["LinkPhonePhone"]?.ToString();
                var emailFromTemp = TempData["LinkPhoneEmail"]?.ToString();

                // Перевіряємо, чи це той самий користувач
                if (user.Email != emailFromTemp)
                {
                    TempData["ErrorMessage"] = "Це не ваш акаунт";
                    return RedirectToAction("Profile");
                }

                if (string.IsNullOrEmpty(phoneToLink))
                {
                    TempData["ErrorMessage"] = "Номер телефону не вказано";
                    return RedirectToAction("Profile");
                }

                // Перевіряємо, чи телефон не зайнятий іншим користувачем
                var phoneUsed = await _agencyDBContext.Users
                    .AnyAsync(u => u.PhoneNumber == phoneToLink && u.Id != userId);

                if (phoneUsed)
                {
                    TempData["ErrorMessage"] = "Цей номер телефону вже використовується іншим користувачем";
                    return View();
                }

                // Прив'язуємо телефон
                user.PhoneNumber = phoneToLink;
                user.PhoneNumberConfirmed = true;
                user.DateOfUpdated = DateTime.Now;

                await _agencyDBContext.SaveChangesAsync();

                // Очищаємо TempData
                TempData.Remove("LinkPhoneEmail");
                TempData.Remove("LinkPhonePhone");
                TempData.Remove("LinkPhoneLogin");

                TempData["SuccessMessage"] = $"Телефон {phoneToLink} успішно прив'язано до вашого акаунта!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LinkPhoneAfterLogin error");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                return View();
            }
        }

        // 📱 Сторінка реєстрації через телефон


        [HttpGet]
        public IActionResult RegisterWithPhone(string? phone = null)
        {
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            TempData.Remove("InfoMessage");

            ViewBag.PhoneNumber = phone;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> RegisterWithPhone(string phoneNumber, string email, string login)
        {
            try
            {
                _logger.LogInformation($"=== REGISTER WITH PHONE START ===");
                _logger.LogInformation($"Phone: {phoneNumber}, Email: {email}, Login: {login}");

                var formattedPhone = FormatPhoneNumber(phoneNumber);
                email = email?.Trim();
                login = login?.Trim();

                // 1. Шукаємо користувача за телефоном (ПІДТВЕРДЖЕНИЙ телефон)
                var existingByPhone = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == formattedPhone && u.PhoneNumberConfirmed);

                if (existingByPhone != null)
                {
                    TempData["ErrorMessage"] = "Цей номер телефону вже зареєстровано. Спробуйте увійти через телефон.";
                    ViewBag.PhoneNumber = phoneNumber;
                    return View();
                }

                // 2. Шукаємо користувача за email (будь-якого)
                var existingByEmail = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (existingByEmail != null)
                {
                    _logger.LogInformation($"Found existing user by email: ID={existingByEmail.Id}, " +
                                         $"GoogleId={existingByEmail.GoogleId}, PhoneConfirmed={existingByEmail.PhoneNumberConfirmed}");

                    // 🔴 ВАЖЛИВО: ПЕРЕВІРКА ДЛЯ ЗВИЧАЙНИХ EMAIL-АКАУНТІВ
                    if (string.IsNullOrEmpty(existingByEmail.GoogleId))
                    {
                        // 3A. Це ЗВИЧАЙНИЙ email-акаунт
                        _logger.LogInformation($"Regular email user found. Checking if we can add phone...");

                        // Перевіряємо, чи вже є прив'язаний телефон
                        if (!string.IsNullOrEmpty(existingByEmail.PhoneNumber))
                        {
                            if (existingByEmail.PhoneNumber == formattedPhone)
                            {
                                // Телефон вже прив'язаний до цього акаунта
                                TempData["ErrorMessage"] = "Цей телефон вже прив'язаний до вашого акаунта.";
                                ViewBag.PhoneNumber = phoneNumber;
                                return View();
                            }
                            else
                            {
                                // До цього акаунта вже прив'язаний інший телефон
                                TempData["ErrorMessage"] = "До вашого акаунта вже прив'язаний інший телефон.";
                                ViewBag.PhoneNumber = phoneNumber;
                                return View();
                            }
                        }

                        //    // 3B. Телефон не прив'язаний - можна додати
                        //    // Але потрібно авторизуватися!
                        //    _logger.LogInformation($"No phone attached. Need authentication to add phone.");

                        //    // Зберігаємо в cookies для подальшої прив'язки
                        //    var cookieOptions = new CookieOptions
                        //    {
                        //        Expires = DateTime.Now.AddMinutes(10),
                        //        HttpOnly = true,
                        //        Secure = false, // Для локальної розробки
                        //        SameSite = SameSiteMode.Lax,
                        //        Path = "/"
                        //    };

                        //    Response.Cookies.Append("LinkPhone_Email", email, cookieOptions);
                        //    Response.Cookies.Append("LinkPhone_Phone", formattedPhone, cookieOptions);
                        //    Response.Cookies.Append("LinkPhone_Login", login ?? existingByEmail.Login, cookieOptions);

                        //    TempData["InfoMessage"] = "Знайдено існуючий акаунт з цим email. " +
                        //                             "Увійдіть, щоб прив'язати телефон до вашого акаунта.";
                        //    return RedirectToAction("ConfirmPhoneLink");
                        //}



                        // 3B. Телефон не прив'язаний - можна додати
                        // Але потрібно верифікація!
                        _logger.LogInformation($"No phone attached. Starting phone verification for linking...");

                        // Перевіряємо, чи телефон не зайнятий іншим користувачем
                        var phoneUsedByOther = await _agencyDBContext.Users
                            .AnyAsync(u => u.PhoneNumber == formattedPhone && u.Id != existingByEmail.Id);

                        if (phoneUsedByOther)
                        {
                            TempData["ErrorMessage"] = "Цей номер телефону вже використовується іншим користувачем";
                            ViewBag.PhoneNumber = phoneNumber;
                            return View();
                        }

                        // Зберігаємо дані для прив'язування
                        var pendingLinkData = new
                        {
                            Email = email,
                            PhoneNumber = formattedPhone,
                            Login = login ?? existingByEmail.Login,
                            UserId = existingByEmail.Id,
                            Action = "LinkPhone" // Маркер для прив'язування
                        };

                        // ⭐⭐⭐ ЗМІНИТИ НАЗВИ ЗМІННИХ ТУТ ⭐⭐⭐
                        var linkDataCacheKey = $"PhoneLinkData_{formattedPhone}"; // ← linkDataCacheKey
                        _cache.Set(linkDataCacheKey, pendingLinkData, TimeSpan.FromMinutes(15));

                        // Генеруємо код верифікації ДЛЯ ПРИВ'ЯЗУВАННЯ
                        var linkVerificationCode = GenerateVerificationCode(); // ← linkVerificationCode
                        var linkCodeCacheKey = $"PhoneLinkCode_{formattedPhone}"; // ← linkCodeCacheKey
                        _cache.Set(linkCodeCacheKey, linkVerificationCode, TimeSpan.FromMinutes(10));

                        _logger.LogInformation($"Phone linking code generated: {linkVerificationCode} for user ID: {existingByEmail.Id}");

                        // Відправляємо SMS з кодом
                        try
                        {
                            await _smsService.SendSmsAsync(formattedPhone,
                                $"Код для прив'язування телефону до акаунта: {linkVerificationCode}. Дійсний 10 хвилин.");
                            TempData["SuccessMessage"] = "Код підтвердження відправлено на ваш телефон";
                        }
                        catch (Exception smsEx)
                        {
                            _logger.LogError(smsEx, "Failed to send SMS");
                            TempData["SuccessMessage"] = $"Тестовий код: {linkVerificationCode}";
                        }

                        // Перенаправляємо на сторінку верифікації
                        TempData["PendingPhoneNumber"] = formattedPhone;
                        TempData["ActionType"] = "LinkPhone"; // Вказуємо тип дії
                        TempData["PendingEmail"] = email;
                        TempData["PendingLogin"] = login ?? existingByEmail.Login;

                        return RedirectToAction("VerifyPhoneLink");



                    }


                    else
                    {
                        // 4. Це GOOGLE-акаунт - прив'язуємо телефон автоматично
                        _logger.LogInformation($"Google user found. Attaching phone...");

                        // Перевіряємо, чи телефон не зайнятий іншим користувачем
                        var phoneUsedByOther = await _agencyDBContext.Users
                            .AnyAsync(u => u.PhoneNumber == formattedPhone && u.Id != existingByEmail.Id);

                        if (phoneUsedByOther)
                        {
                            TempData["ErrorMessage"] = "Цей номер телефону вже використовується іншим користувачем";
                            ViewBag.PhoneNumber = phoneNumber;
                            return View();
                        }

                        // Прив'язуємо телефон до існуючого Google-акаунту
                        existingByEmail.PhoneNumber = formattedPhone;
                        existingByEmail.PhoneNumberConfirmed = true;
                        existingByEmail.DateOfUpdated = DateTime.Now;

                        await _agencyDBContext.SaveChangesAsync();

                        // Авторизуємо користувача
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, existingByEmail.Id.ToString()),
                    new Claim(ClaimTypes.Email, existingByEmail.Email),
                    new Claim(ClaimTypes.Name, existingByEmail.Login),
                    new Claim("UserId", existingByEmail.Id.ToString()),
                    new Claim("Login", existingByEmail.Login)
                };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal,
                            new AuthenticationProperties
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTime.UtcNow.AddDays(30)
                            });

                        TempData["SuccessMessage"] = "Телефон успішно прив'язано до вашого Google-акаунта!";
                        return RedirectToAction("Profile", "Account");
                    }
                }

                // 5. Перевірка логіну (тільки якщо користувача з email не знайдено)
                var existingByLogin = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.Login == login);

                if (existingByLogin != null)
                {
                    TempData["ErrorMessage"] = "Цей логін вже зайнятий";
                    ViewBag.PhoneNumber = phoneNumber;
                    return View();
                }

                // 6. Перевірка, чи телефон не використовується (непідтверджений)
                var existingUnconfirmedPhone = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == formattedPhone && !u.PhoneNumberConfirmed);

                if (existingUnconfirmedPhone != null)
                {
                    // Телефон є, але не підтверджений - пропонуємо підтвердити
                    TempData["ErrorMessage"] = "Цей номер телефону вже використовується, але не підтверджений. " +
                                              "Спробуйте увійти через телефон для підтвердження.";
                    ViewBag.PhoneNumber = phoneNumber;
                    return View();
                }





                // ⭐⭐⭐ НОВА ЛОГІКА: ЗБЕРІГАЄМО ДАНІ ТА ВІДПРАВЛЯЄМО КОД ⭐⭐⭐

                // 7A. Генеруємо код верифікації
                var verificationCode = GenerateVerificationCode();

                // 7B. Зберігаємо дані реєстрації в тимчасовому сховищі
                var pendingRegistration = new
                {
                    PhoneNumber = formattedPhone,
                    Email = email,
                    Login = login,
                    // Можна додати таймштамп
                    CreatedAt = DateTime.Now
                };

                // Ключі для кешу
                var codeCacheKey = $"PhoneRegCode_{formattedPhone}";
                var dataCacheKey = $"PhoneRegData_{formattedPhone}";

                // Зберігаємо код на 10 хвилин
                _cache.Set(codeCacheKey, verificationCode, TimeSpan.FromMinutes(10));
                // Зберігаємо дані реєстрації на 15 хвилин
                _cache.Set(dataCacheKey, pendingRegistration, TimeSpan.FromMinutes(15));

                _logger.LogInformation($"Verification code generated: {verificationCode}");

                // 7C. Відправляємо SMS з кодом
                try
                {
                    await _smsService.SendSmsAsync(formattedPhone,
                        $"Код для завершення реєстрації: {verificationCode}. Дійсний 10 хвилин.");
                    TempData["SuccessMessage"] = "Код підтвердження відправлено на ваш телефон";
                }
                catch (Exception smsEx)
                {
                    _logger.LogError(smsEx, "Failed to send SMS");
                    TempData["SuccessMessage"] = $"Тестовий код: {verificationCode}";
                }

                // 7D. Зберігаємо дані для наступної сторінки
                TempData["PendingPhoneNumber"] = formattedPhone;
                TempData["PendingEmail"] = email;
                TempData["PendingLogin"] = login;

                // 7E. Перенаправляємо на сторінку верифікації
                return RedirectToAction("VerifyPhoneRegistration");

                // ⭐⭐⭐ КІНЕЦЬ НОВОЇ ЛОГІКИ ⭐⭐⭐




                //        // 7. Створення НОВОГО користувача (якщо email, телефон і логін вільні)
                //        var user = new User
                //        {
                //            Email = email,
                //            Login = login,
                //            PhoneNumber = formattedPhone,
                //            PhoneNumberConfirmed = true,
                //            PasswordHash = SecurePasswordHasher.Hash(Guid.NewGuid().ToString()),
                //            DateOfCreat = DateTime.Now,
                //        };

                //        _agencyDBContext.Users.Add(user);
                //        await _agencyDBContext.SaveChangesAsync();

                //        // Авторизація нового користувача
                //        var claims2 = new List<Claim>
                //{
                //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                //    new Claim(ClaimTypes.Email, user.Email ?? ""),
                //    new Claim(ClaimTypes.Name, user.Login),
                //    new Claim("UserId", user.Id.ToString()),
                //    new Claim("Login", user.Login)
                //};

                //        var identity2 = new ClaimsIdentity(claims2, CookieAuthenticationDefaults.AuthenticationScheme);
                //        var principal2 = new ClaimsPrincipal(identity2);

                //        await HttpContext.SignInAsync(
                //            CookieAuthenticationDefaults.AuthenticationScheme,
                //            principal2,
                //            new AuthenticationProperties
                //            {
                //                IsPersistent = true,
                //                ExpiresUtc = DateTime.UtcNow.AddDays(30)
                //            });

                //        TempData["SuccessMessage"] = "Реєстрація через телефон успішна! Ласкаво просимо!";
                //        return RedirectToAction("Welcome");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при реєстрації через телефон");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                ViewBag.PhoneNumber = phoneNumber;
                return View();
            }
        }





        // GET: Сторінка верифікації для прив'язування телефону
        [HttpGet]
        public IActionResult VerifyPhoneLink()
        {
            if (TempData["ActionType"]?.ToString() != "LinkPhone")
            {
                TempData["ErrorMessage"] = "Невірний запит";
                return RedirectToAction("LoginIn");
            }

            return View();
        }

        // POST: Обробка верифікації для прив'язування
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneLink(string phoneNumber, string code)
        {
            try
            {
                _logger.LogInformation($"=== VERIFY PHONE LINK POST ===");
                _logger.LogInformation($"Verifying code {code} for phone link {phoneNumber}");

                var formattedPhone = FormatPhoneNumber(phoneNumber);

                var linkCodeCacheKey = $"PhoneLinkCode_{formattedPhone}";
                var linkDataCacheKey = $"PhoneLinkData_{formattedPhone}";

                // 1. Перевіряємо код
                if (!_cache.TryGetValue(linkCodeCacheKey, out string storedCode) || storedCode != code)
                {
                    TempData["ErrorMessage"] = $"Невірний код або час вийшов";
                    TempData["PendingPhoneNumber"] = phoneNumber;
                    TempData["ActionType"] = "LinkPhone";
                    return View();
                }

                // 2. Отримуємо дані для прив'язування
                if (!_cache.TryGetValue(linkDataCacheKey, out dynamic linkData))
                {
                    TempData["ErrorMessage"] = "Час на прив'язування телефону вийшов";
                    return RedirectToAction("LoginIn");
                }

                // 3. Знаходимо користувача
                var user = await _agencyDBContext.Users.FindAsync((int)linkData.UserId);
                if (user == null || user.Email != linkData.Email.ToString())
                {
                    TempData["ErrorMessage"] = "Користувача не знайдено";
                    return RedirectToAction("LoginIn");
                }

                // 4. Перевіряємо, чи телефон не зайнятий іншим користувачем
                var phoneUsedByOther = await _agencyDBContext.Users
                    .AnyAsync(u => u.PhoneNumber == formattedPhone && u.Id != user.Id);

                if (phoneUsedByOther)
                {
                    TempData["ErrorMessage"] = "Цей номер телефону вже використовується іншим користувачем";
                    TempData["PendingPhoneNumber"] = phoneNumber;
                    TempData["ActionType"] = "LinkPhone";
                    return View();
                }

                // 5. Прив'язуємо телефон до існуючого акаунта
                user.PhoneNumber = formattedPhone;
                user.PhoneNumberConfirmed = true; // ✅ Телефон верифікований
                user.DateOfUpdated = DateTime.Now;

                await _agencyDBContext.SaveChangesAsync();

                _logger.LogInformation($"✅ Phone {formattedPhone} linked to user: ID={user.Id}, Email={user.Email}");

                // 6. Очищаємо кеш
                _cache.Remove(linkCodeCacheKey);
                _cache.Remove(linkDataCacheKey);

                // 7. Автоматично авторизуємо користувача
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("UserId", user.Id.ToString()),
            new Claim("Login", user.Login),
            new Claim(ClaimTypes.MobilePhone, formattedPhone),
            new Claim("PhoneConfirmed", "true")
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30)
                    });

                // 8. Показуємо повідомлення про успіх
                TempData["SuccessMessage"] = $"Телефон {formattedPhone} успішно прив'язано до вашого акаунта!";

                // 9. Перенаправляємо на сторінку підтвердження
                TempData["LinkedPhone"] = formattedPhone;
                TempData["LinkedEmail"] = user.Email;

                return RedirectToAction("PhoneLinkSuccess");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyPhoneLink error");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                TempData["PendingPhoneNumber"] = phoneNumber;
                TempData["ActionType"] = "LinkPhone";
                return View();
            }
        }




        [HttpGet]
        public IActionResult PhoneLinkSuccess()
        {
            if (TempData["LinkedPhone"] == null)
            {
                return RedirectToAction("LoginIn");
            }

            return View();
        }









        // GET: Сторінка для введення коду верифікації при реєстрації
        [HttpGet]
        public IActionResult VerifyPhoneRegistration()
        {
            return View();
        }

        // POST: Обробка коду верифікації та створення користувача
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneRegistration(string phoneNumber, string code)
        {
            try
            {
                _logger.LogInformation($"=== VERIFY PHONE REGISTRATION POST ===");
                _logger.LogInformation($"Verifying code {code} for registration phone {phoneNumber}");

                var formattedPhone = FormatPhoneNumber(phoneNumber);
                var codeCacheKey = $"PhoneRegCode_{formattedPhone}";
                var dataCacheKey = $"PhoneRegData_{formattedPhone}";

                // 1. Перевіряємо код
                if (!_cache.TryGetValue(codeCacheKey, out string storedCode) || storedCode != code)
                {
                    TempData["ErrorMessage"] = $"Невірний код або час вийшов. Очікував: {storedCode}, отримав: {code}";
                    TempData["PendingPhoneNumber"] = phoneNumber;
                    return View();
                }

                // 2. Отримуємо дані реєстрації
                if (!_cache.TryGetValue(dataCacheKey, out dynamic registrationData))
                {
                    TempData["ErrorMessage"] = "Час на реєстрацію вийшов. Спробуйте знову.";
                    return RedirectToAction("RegisterWithPhone");
                }

                // 3. Створюємо користувача
                var user = new User
                {
                    Email = registrationData.Email,
                    Login = registrationData.Login,
                    PhoneNumber = formattedPhone,
                    PhoneNumberConfirmed = true, // ✅ Тепер номер ПІДТВЕРДЖЕНИЙ
                    PasswordHash = SecurePasswordHasher.Hash(Guid.NewGuid().ToString()),
                    DateOfCreat = DateTime.Now,
                };

                _agencyDBContext.Users.Add(user);
                await _agencyDBContext.SaveChangesAsync();

                _logger.LogInformation($"✅ New user created after phone verification: ID={user.Id}, Email={user.Email}");

                // 4. Авторизуємо користувача
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("UserId", user.Id.ToString()),
            new Claim("Login", user.Login),
            new Claim("PhoneNumber", user.PhoneNumber ?? "")
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30)
                    });

                // 5. Очищаємо кеш
                _cache.Remove(codeCacheKey);
                _cache.Remove(dataCacheKey);

                // 6. Видаляємо тимчасові дані
                TempData.Remove("PendingPhoneNumber");
                TempData.Remove("PendingEmail");
                TempData.Remove("PendingLogin");

                _logger.LogInformation($"✅ User {user.Email} successfully registered and logged in");

                TempData["SuccessMessage"] = "Реєстрація через телефон успішна! Ласкаво просимо!";
                return RedirectToAction("Welcome");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyPhoneRegistration error");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                TempData["PendingPhoneNumber"] = phoneNumber;
                return View();
            }
        }

        // ========== ДОДАВАННЯ ТЕЛЕФОНУ ДО ПРОФІЛЮ ==========

        // 📱 Додати телефон до профілю
        [Authorize]
        [HttpGet]
        public IActionResult AddPhone()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPhone(string phoneNumber)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var user = await _agencyDBContext.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Користувача не знайдено";
                    return View();
                }

                // Форматуємо номер
                var formattedPhone = FormatPhoneNumber(phoneNumber);

                // Перевірка на унікальність
                var existingUser = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == formattedPhone && u.Id != userId);

                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Цей номер телефону вже використовується";
                    return View();
                }

                // Зберігаємо номер
                user.PhoneNumber = formattedPhone;
                user.PhoneNumberConfirmed = false;
                
                user.DateOfUpdated = DateTime.Now;

                await _agencyDBContext.SaveChangesAsync();

                // Відправляємо код підтвердження
                await SendPhoneVerificationCode(user);

                TempData["SuccessMessage"] = "Код підтвердження відправлено на ваш телефон";
                return RedirectToAction("VerifyPhone");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при додаванні телефону");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                return View();
            }
        }

        // ✅ Підтвердити телефон
        [Authorize]
        [HttpGet]
        public IActionResult VerifyPhone()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> VerifyPhone(string code)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var user = await _agencyDBContext.Users.FindAsync(userId);

                if (user == null || string.IsNullOrEmpty(user.PhoneNumber))
                {
                    TempData["ErrorMessage"] = "Телефон не знайдено";
                    return View();
                }

                // Отримуємо код з кешу
                var cacheKey = $"PhoneVerify_{user.PhoneNumber}";
                if (!_cache.TryGetValue(cacheKey, out string storedCode) || storedCode != code)
                {
                    TempData["ErrorMessage"] = "Невірний код або час вийшов";
                    return View();
                }

                // Підтверджуємо телефон
                user.PhoneNumberConfirmed = true;
                user.DateOfUpdated = DateTime.Now;

                await _agencyDBContext.SaveChangesAsync();

                // Очищаємо кеш
                _cache.Remove(cacheKey);

                TempData["SuccessMessage"] = "Телефон успішно підтверджено!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при підтвердженні телефону");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                return View();
            }
        }

        // 👤 Профіль користувача
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var user = await _agencyDBContext.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            var model = new UserProfileViewModel
            {
                Email = user.Email,
                Login = user.Login,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                GoogleId = user.GoogleId,
                AuthType = GetAuthType(user)
            };

            return View(model);
        }

        // 🗑️ Видалити телефон з профілю
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemovePhone()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var user = await _agencyDBContext.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Користувача не знайдено";
                    return RedirectToAction("Profile");
                }

                user.PhoneNumber = null;
                user.PhoneNumberConfirmed = false;
                user.DateOfUpdated = DateTime.Now;

                await _agencyDBContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Телефон видалено з профілю";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при видаленні телефону");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }




        private async Task<(bool exists, string message)> CheckUserExists(string email, string login, string phone = null)
        {
            var existingUser = await _agencyDBContext.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == email ||
                    u.Login == login ||
                    (phone != null && u.PhoneNumber == phone));

            if (existingUser == null)
                return (false, null);

            if (existingUser.Email == email)
            {
                if (!string.IsNullOrEmpty(existingUser.GoogleId))
                    return (true, "Цей email вже зареєстровано через Google. Використовуйте кнопку 'Увійти через Google'");
                else
                    return (true, "Користувач з таким email вже існує");
            }

            if (existingUser.Login == login)
                return (true, "Користувач з таким логіном вже існує");

            if (phone != null && existingUser.PhoneNumber == phone)
                return (true, "Користувач з таким номером телефону вже існує");

            return (false, null);
        }





        // GET: Показати форму прив'язування
        [HttpGet]
        public IActionResult LinkPasswordToGoogle(string fromProfile = null)
        {
            // Якщо перейшли з профілю авторизованого користувача
            if (fromProfile == "true" && (User.Identity?.IsAuthenticated ?? false))
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = _agencyDBContext.Users.Find(int.Parse(userId));

                    if (user != null && !string.IsNullOrEmpty(user.GoogleId))
                    {
                        TempData["LinkAccountEmail"] = user.Email;
                        TempData["LinkAccountLogin"] = user.Login;
                        TempData["FromProfile"] = "true";

                        return View();
                    }
                }
            }

            // Стара логіка для спроб реєстрації з існуючим Google email
            if (TempData["LinkAccountEmail"] == null)
            {
                TempData["ErrorMessage"] = "Сесія закінчилася";
                return RedirectToAction("LoginIn");
            }

            return View();
        }



        // POST: Обробити прив'язування пароля до Google-акаунта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkPassword(string email, string login, string password, string confirmPassword)
        {
            try
            {
                _logger.LogInformation($"=== LINK PASSWORD START ===");
                _logger.LogInformation($"Email: {email}, Login: {login}");

                // 1. Базові перевірки
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["ErrorMessage"] = "Email не вказано";
                    TempData["LinkAccountEmail"] = email;
                    TempData["LinkAccountLogin"] = login;
                    return RedirectToAction("LinkPasswordToGoogle");
                }

                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    TempData["ErrorMessage"] = "Пароль має містити мінімум 6 символів";
                    TempData["LinkAccountEmail"] = email;
                    TempData["LinkAccountLogin"] = login;
                    return RedirectToAction("LinkPasswordToGoogle");
                }

                if (password != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Паролі не співпадають";
                    TempData["LinkAccountEmail"] = email;
                    TempData["LinkAccountLogin"] = login;
                    return RedirectToAction("LinkPasswordToGoogle");
                }

                // 2. Знайти користувача з відстеженням
                var user = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.LogWarning($"User not found for email: {email}");
                    TempData["ErrorMessage"] = "Користувача не знайдено";
                    return RedirectToAction("LoginIn");
                }

                _logger.LogInformation($"User found: ID={user.Id}, Email={user.Email}, GoogleId={user.GoogleId}");
                _logger.LogInformation($"Current hash exists: {!string.IsNullOrEmpty(user.PasswordHash)}");
                _logger.LogInformation($"Current DateOfUpdated: {user.DateOfUpdated}");

                // 3. Перевірити, чи це Google-акаунт
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    TempData["ErrorMessage"] = "Цей акаунт не зареєстровано через Google. Використовуйте звичайний вхід.";
                    return RedirectToAction("LoginIn");
                }

                // 4. ЗАМІНИТИ автоматично згенерований Google пароль на пароль користувача
                // ❗ Важливо: НЕ перевіряємо чи є вже пароль - Google завжди генерує пароль
                // Ми просто замінюємо його на пароль, який знає користувач

                var oldHash = user.PasswordHash;
                var newHash = SecurePasswordHasher.Hash(password);

                _logger.LogInformation($"Old hash: {oldHash}");
                _logger.LogInformation($"New hash: {newHash}");

                // ЗАМІНА пароля
                user.PasswordHash = newHash;

                // Оновити логін, якщо потрібно
                if (!string.IsNullOrWhiteSpace(login) && user.Login != login)
                {
                    user.Login = login;
                    _logger.LogInformation($"Login updated to: {login}");
                }

                user.DateOfUpdated = DateTime.Now;
                _logger.LogInformation($"DateOfUpdated set to: {user.DateOfUpdated}");

                // 5. ЗБЕРІГТИ зміни в базі даних
                try
                {
                    _agencyDBContext.Users.Update(user);
                    var saveResult = await _agencyDBContext.SaveChangesAsync();
                    _logger.LogInformation($"SaveChanges result: {saveResult} rows affected");

                    // Перевірити, що зміни збереглися
                    var savedUser = await _agencyDBContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == user.Id);

                    if (savedUser != null)
                    {
                        _logger.LogInformation($"After save - Hash: {savedUser.PasswordHash?.Substring(0, 30)}...");
                        _logger.LogInformation($"After save - DateOfUpdated: {savedUser.DateOfUpdated}");

                        // Перевірити, що пароль дійсно змінився
                        var isNewPasswordValid = SecurePasswordHasher.Verify(password, savedUser.PasswordHash);
                        _logger.LogInformation($"New password verification: {isNewPasswordValid}");

                        if (!isNewPasswordValid)
                        {
                            _logger.LogError("Password was not saved correctly!");
                            TempData["ErrorMessage"] = "Помилка при збереженні пароля. Спробуйте ще раз.";
                            return RedirectToAction("LinkPasswordToGoogle");
                        }
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database update error");
                    TempData["ErrorMessage"] = "Помилка бази даних. Спробуйте ще раз.";
                    TempData["LinkAccountEmail"] = email;
                    TempData["LinkAccountLogin"] = login;
                    return RedirectToAction("LinkPasswordToGoogle");
                }

                // 6. Автоматично увійти після успішної зміни пароля
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("UserId", user.Id.ToString()),
            new Claim("Login", user.Login),
            new Claim("HasCustomPassword", "true") // Маркер, що пароль встановлений користувачем
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30)
                    });

                _logger.LogInformation($"User {user.Email} successfully logged in with new password");

                // 7. Повідомити про успіх і перенаправити
                TempData["SuccessMessage"] = $"Пароль успішно встановлено для вашого Google-акаунта! Тепер ви можете входити через email та пароль.";

                // Встановити флаг для відображення в профілі
                TempData["PasswordSetSuccess"] = "true";

                // Перевірити, чи адміністратор
                if (IsAdmin(user))
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LinkPassword error");
                TempData["ErrorMessage"] = $"Сталася помилка при встановленні пароля: {ex.Message}";

                // Повернути дані для повторної спроби
                if (!string.IsNullOrWhiteSpace(email))
                {
                    TempData["LinkAccountEmail"] = email;
                    TempData["LinkAccountLogin"] = login;
                }

                return RedirectToAction("LinkPasswordToGoogle");
            }
            finally
            {
                _logger.LogInformation($"=== LINK PASSWORD END ===");
            }
        }


        [HttpGet]
        public async Task<IActionResult> DebugUser(string email)
        {
            try
            {
                var user = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return Content($"Користувача {email} не знайдено");
                }

                return Content($"ID: {user.Id}<br>" +
                               $"Email: {user.Email}<br>" +
                               $"Login: {user.Login}<br>" +
                               $"GoogleId: {user.GoogleId}<br>" +
                               $"Phone: {user.PhoneNumber}<br>" +
                               $"Є пароль: {!string.IsNullOrEmpty(user.PasswordHash)}<br>" +
                               $"Hash: {user.PasswordHash}");
            }
            catch (Exception ex)
            {
                return Content($"Помилка: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestLinkPassword(string email, string password = "123456")
        {
            try
            {
                var user = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                    return Content($"Користувача {email} не знайдено");

                var oldHash = user.PasswordHash;

                // Змінюємо пароль
                user.PasswordHash = SecurePasswordHasher.Hash(password);
                user.DateOfUpdated = DateTime.Now;

                await _agencyDBContext.SaveChangesAsync();

                // Перезавантажуємо з бази
                await _agencyDBContext.Entry(user).ReloadAsync();

                return Content($"Email: {user.Email}<br>" +
                               $"GoogleId: {user.GoogleId}<br>" +
                               $"Старий хеш: {oldHash}<br>" +
                               $"Новий хеш: {user.PasswordHash}<br>" +
                               $"DateOfUpdated: {user.DateOfUpdated}<br>" +
                               $"Успішно оновлено!");
            }
            catch (Exception ex)
            {
                return Content($"Помилка: {ex.Message}<br>{ex.StackTrace}");
            }
        }









        [HttpGet]
        public async Task<IActionResult> DebugPhoneCache(string phoneNumber)
        {
            try
            {
                var formattedPhone = FormatPhoneNumber(phoneNumber);
                var cacheKey = $"PhoneLogin_{formattedPhone}";
                var userCacheKey = $"PhoneUser_{formattedPhone}";

                var hasCode = _cache.TryGetValue(cacheKey, out string code);
                var hasUserId = _cache.TryGetValue(userCacheKey, out int userId);

                var user = await _agencyDBContext.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == formattedPhone);

                return Json(new
                {
                    Phone = formattedPhone,
                    HasCodeInCache = hasCode,
                    Code = code,
                    HasUserIdInCache = hasUserId,
                    UserId = userId,
                    UserInDb = user != null ? new
                    {
                        user.Id,
                        user.Email,
                        user.Login,
                        user.PhoneNumber,
                        user.PhoneNumberConfirmed
                    } : null
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }



        [HttpGet]
        public async Task<IActionResult> TestPassword(string email, string password)
        {
            var user = await _agencyDBContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return Content($"❌ Користувача {email} не знайдено");

            var hasPassword = !string.IsNullOrEmpty(user.PasswordHash);
            var isCorrect = hasPassword && SecurePasswordHasher.Verify(password, user.PasswordHash);

            return Content($"Email: {user.Email}<br>" +
                           $"Login: {user.Login}<br>" +
                           $"GoogleId: {user.GoogleId}<br>" +
                           $"HasPassword: {hasPassword}<br>" +
                           $"PasswordHash: {user.PasswordHash?.Substring(0, 30)}...<br>" +
                           $"Test Password: {password}<br>" +
                           $"IsCorrect: {isCorrect}");
        }









        [HttpGet]
        public IActionResult ConfirmPhoneLink()
        {
            // Отримуємо дані з cookies
            var email = Request.Cookies["LinkPhone_Email"];
            var phone = Request.Cookies["LinkPhone_Phone"];
            var login = Request.Cookies["LinkPhone_Login"];

            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Сесія закінчилася. Спробуйте знову.";
                return RedirectToAction("RegisterWithPhone");
            }

            ViewBag.LinkPhoneEmail = email;
            ViewBag.LinkPhonePhone = phone;
            ViewBag.LinkPhoneLogin = login;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPhoneLink(string confirm)
        {
            try
            {
                var email = Request.Cookies["LinkPhone_Email"];
                var phone = Request.Cookies["LinkPhone_Phone"];
                var login = Request.Cookies["LinkPhone_Login"];

                if (string.IsNullOrEmpty(email))
                {
                    TempData["ErrorMessage"] = "Сесія закінчилася.";
                    return RedirectToAction("RegisterWithPhone");
                }

                if (confirm == "yes")
                {
                    // Знаходимо користувача
                    var user = await _agencyDBContext.Users
                        .FirstOrDefaultAsync(u => u.Email == email);

                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "Користувача не знайдено";
                        return RedirectToAction("LoginIn");
                    }

                    // Перевіряємо, чи телефон не зайнятий
                    var phoneUsedByOther = await _agencyDBContext.Users
                        .AnyAsync(u => u.PhoneNumber == phone && u.Id != user.Id);

                    if (phoneUsedByOther)
                    {
                        TempData["ErrorMessage"] = "Цей номер телефону вже використовується іншим користувачем";
                        return View();
                    }

                    // Прив'язуємо телефон
                    user.PhoneNumber = phone;
                    user.PhoneNumberConfirmed = true;
                    user.DateOfUpdated = DateTime.Now;

                    // Оновлюємо логін, якщо потрібно
                    if (!string.IsNullOrEmpty(login) && user.Login != login)
                    {
                        user.Login = login;
                    }

                    await _agencyDBContext.SaveChangesAsync();

                    // Автоматично входимо
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim("UserId", user.Id.ToString()),
                new Claim("Login", user.Login)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddDays(30)
                        });

                    // Очищаємо cookies
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Path = "/"
                    };

                    Response.Cookies.Append("LinkPhone_Email", "", cookieOptions);
                    Response.Cookies.Append("LinkPhone_Phone", "", cookieOptions);
                    Response.Cookies.Append("LinkPhone_Login", "", cookieOptions);

                    TempData["SuccessMessage"] = $"Телефон {phone} успішно прив'язано до вашого акаунта!";
                    return RedirectToAction("Profile");
                }
                else
                {
                    // Очищаємо cookies
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Path = "/"
                    };

                    Response.Cookies.Append("LinkPhone_Email", "", cookieOptions);
                    Response.Cookies.Append("LinkPhone_Phone", "", cookieOptions);
                    Response.Cookies.Append("LinkPhone_Login", "", cookieOptions);

                    TempData["InfoMessage"] = "Дію скасовано";
                    return RedirectToAction("RegisterWithPhone");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConfirmPhoneLink error");
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                return View();
            }
        }




        //[HttpGet]
        //public async Task<IActionResult> TestPhoneLink(string phone = "+380667160527", string email = "koshka20.03.19@gmail.com")
        //{
        //    // Тестовий метод для перевірки прив'язування телефону
        //    var user = await _agencyDBContext.Users
        //        .FirstOrDefaultAsync(u => u.Email == email);

        //    if (user == null)
        //        return Content($"Користувача {email} не знайдено");

        //    var phoneToLink = FormatPhoneNumber(phone);

        //    // Встановлюємо cookies для тесту
        //    var cookieOptions = new CookieOptions
        //    {
        //        Expires = DateTime.Now.AddMinutes(10),
        //        HttpOnly = true,
        //        Secure = false,
        //        SameSite = SameSiteMode.Lax,
        //        Path = "/"
        //    };

        //    Response.Cookies.Append("LinkPhone_Email", email, cookieOptions);
        //    Response.Cookies.Append("LinkPhone_Phone", phoneToLink, cookieOptions);
        //    Response.Cookies.Append("LinkPhone_Login", user.Login, cookieOptions);

        //    return RedirectToAction("ConfirmPhoneLink");
        //}










        //[HttpGet]
        //public async Task<IActionResult> SendTestSms()
        //{
        //    try
        //    {
        //        // Використовуйте ваш підтверджений номер телефону
        //        // (той, на який приходили SMS від Twilio)
        //        var myVerifiedNumber = "+380667160527";

        //        var result = await _smsService.SendSmsAsync(
        //            myVerifiedNumber,
        //            $"✅ Twilio тест з номера +12054798675\n" +
        //            $"Код: {new Random().Next(100000, 999999)}\n" +
        //            $"Час: {DateTime.Now:T}"
        //        );

        //        return Json(new
        //        {
        //            success = result,
        //            message = result ? "SMS відправлено!" : "Помилка відправки",
        //            toNumber = myVerifiedNumber,
        //            fromNumber = "+12054798675"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            error = ex.Message
        //        });
        //    }
        //}




        //[HttpGet]
        //public async Task<IActionResult> DemoSms()
        //{
        //    var phone = "+380667160527"; // Ваш номер для тесту

        //    _logger.LogInformation("=== ДЕМО SMS-АВТОРИЗАЦІЇ ===");

        //    // Відправляємо код
        //    var code = await ((MockSmsService)_smsService).SendVerificationCodeAsync(phone);

        //    // Отримуємо останнє повідомлення
        //    var lastMessage = _cache.Get<string>($"LastSMS_{phone}");
        //    var storedCode = _cache.Get<string>($"SMSCode_{phone}");

        //    return Json(new
        //    {
        //        success = true,
        //        phone = phone,
        //        sentCode = code,
        //        storedCode = storedCode,
        //        lastMessage = lastMessage,
        //        timestamp = DateTime.Now,
        //        note = "✅ Демо-режим активовано. SMS не відправляються реально."
        //    });
        //}




        //[HttpGet]
        //public async Task<IActionResult> SendVerificationCode(string phone)
        //{
        //    try
        //    {
        //        // Використовуємо Mock сервіс
        //        var code = await ((MockSmsService)_smsService).SendVerificationCodeAsync(phone);

        //        return Json(new
        //        {
        //            success = true,
        //            phone = phone,
        //            code = code,
        //            note = "Демо-режим: код збережено в кеші"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = ex.Message });
        //    }
        //}

        //[HttpGet]
        //public IActionResult VerifyCode(string phone, string code)
        //{
        //    var storedCode = _cache.Get<string>($"SMSCode_{phone}");
        //    var isValid = storedCode == code;

        //    if (isValid)
        //    {
        //        // Очищаємо код після успішної перевірки
        //        _cache.Remove($"SMSCode_{phone}");
        //    }

        //    return Json(new { valid = isValid });
        //}

        //[HttpGet]
        //public IActionResult GetSmsLog()
        //{
        //    try
        //    {
        //        // Використовуйте повний шлях System.IO.File
        //        var logPath = Path.Combine("wwwroot", "sms_log.txt");

        //        if (System.IO.File.Exists(logPath))
        //        {
        //            var logContent = System.IO.File.ReadAllText(logPath);
        //            return Content(logContent);
        //        }
        //        return Content("Лог ще порожній");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Помилка читання логу SMS");
        //        return Content("Помилка читання логу: " + ex.Message);
        //    }
        //}




    }



}








    

