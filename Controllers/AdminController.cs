using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WebApp.Db;
using WebApp.Entities;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {

        private readonly AgencyDBContext _agencyDBContext;

        private OptionModels _optionModels;

        public AdminController(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
            _optionModels = new OptionModels(_agencyDBContext);
        }




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


        public IActionResult Config()
        {
            return View(_optionModels.GetAllOptions());
        }



        public IActionResult EditOption(int optionId)
        {

            EditedViewOptionModel editedOptionView = new EditedViewOptionModel();
            editedOptionView.Option = _optionModels.GetOptionById(optionId);
            editedOptionView.Relations = _optionModels.GetUniqueRelations();

            return View(editedOptionView);
        }





        // GET: Форма для додавання нового Relation
        [HttpGet]
        public IActionResult AddNewRelation()
        {
            return View();
        }



        // POST: Обробка додавання нового Relation
        [HttpPost]
        public IActionResult AddNewRelation(string newRelation)
        {
            if (string.IsNullOrEmpty(newRelation))
            {
                TempData["ErrorMessage"] = "Назва нової групи не може бути порожньою.";
                return View();
            }
            try
            {
                // Додаємо нове Relation
                _optionModels.AddNewRelation(newRelation);
                TempData["SuccessMessage"] = "Нова група опцій успішно додана.";
                return RedirectToAction("Config");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка при додаванні нової групи опцій: " + ex.Message;
                return View();
            }
        }
        [HttpPost]
        public IActionResult UpdateOption(Option Option)
        {
            try
            {
                Console.WriteLine("=== UPDATE OPTION WITH OPTION MODEL ===");
                Console.WriteLine($"ID: {Option?.Id}");
                Console.WriteLine($"Name: {Option?.Name}");
                Console.WriteLine($"Key: {Option?.Key}");
                Console.WriteLine($"Value: {Option?.Value}");
                Console.WriteLine($"Relation: {Option?.Relation}");
                Console.WriteLine($"Order: {Option?.Order}");
                Console.WriteLine($"IsSystem: {Option?.IsSystem}");

                // Перевірка обов'язкових полів
                if (Option?.Id == 0)
                {
                    TempData["ErrorMessage"] = "ID опції не вказано";
                    return RedirectToAction("Config");
                }

                _optionModels.UpdateOption(Option);
                TempData["SuccessMessage"] = "Опцію успішно оновлено.";
                return RedirectToAction("Config");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");

                TempData["ErrorMessage"] = "Помилка при оновленні опції: " + ex.Message;

                // Повертаємо на сторінку редагування з поточними даними
                var model = new EditedViewOptionModel
                {
                    Option = _optionModels.GetOptionById(Option?.Id ?? 0),
                    Relations = _optionModels.GetUniqueRelations()
                };
                return View("EditOption", model);
            }
        }




        [HttpPost]
        public IActionResult DeleteOption(int id)
        {
            try
            {
                var option = _optionModels.GetOptionById(id);

                if (option == null)
                {
                    TempData["ErrorMessage"] = "Опція не знайдена";
                    return RedirectToAction("Config");
                }

                if (option.IsSystem)
                {
                    TempData["ErrorMessage"] = "Системні опції не можна видаляти";
                    return RedirectToAction("Config");
                }

                _agencyDBContext.Options.Remove(option);
                _agencyDBContext.SaveChanges();

                TempData["SuccessMessage"] = "Опцію успішно видалено";
                return RedirectToAction("Config");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка при видаленні опції: " + ex.Message;
                return RedirectToAction("Config");
            }
        }

    }
}
