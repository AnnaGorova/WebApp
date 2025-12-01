using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
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





        //public IActionResult Categories()
        //{
        //    return View();
        //}


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




        //[HttpPost]
        //public IActionResult DeleteOption(int id)
        //{
        //    try
        //    {
        //        var option = _optionModels.GetOptionById(id);

        //        if (option == null)
        //        {
        //            TempData["ErrorMessage"] = "Опція не знайдена";
        //            return RedirectToAction("Config");
        //        }

        //        if (option.IsSystem)
        //        {
        //            TempData["ErrorMessage"] = "Системні опції не можна видаляти";
        //            return RedirectToAction("Config");
        //        }

        //        _agencyDBContext.Options.Remove(option);
        //        _agencyDBContext.SaveChanges();

        //        TempData["SuccessMessage"] = "Опцію успішно видалено";
        //        return RedirectToAction("Config");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Помилка при видаленні опції: " + ex.Message;
        //        return RedirectToAction("Config");
        //    }
        //}



        [HttpPost]
        public IActionResult DeleteOption(int id)
        {
            try
            {
                Console.WriteLine($"=== DELETE OPTION ATTEMPT ===");
                Console.WriteLine($"Option ID: {id}");

                var option = _optionModels.GetOptionById(id);

                if (option == null)
                {
                    Console.WriteLine($"❌ Option not found: {id}");
                    TempData["ErrorMessage"] = "Опція не знайдена";
                    return RedirectToAction("Config");
                }

                if (option.IsSystem)
                {
                    Console.WriteLine($"❌ System option cannot be deleted: {id}");
                    TempData["ErrorMessage"] = "Системні опції не можна видаляти";
                    return RedirectToAction("Config");
                }

                Console.WriteLine($"✅ Deleting option: {option.Name} (ID: {option.Id})");

                _agencyDBContext.Options.Remove(option);
                _agencyDBContext.SaveChanges();

                Console.WriteLine($"✅ Option deleted successfully");
                TempData["SuccessMessage"] = "Опцію успішно видалено";
                return RedirectToAction("Config");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting option: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка при видаленні опції: " + ex.Message;
                return RedirectToAction("Config");
            }
        }









        [HttpPost]
        public IActionResult AddOption(Option newOption)
        {
            try
            {
                Console.WriteLine("=== ADD NEW OPTION ===");
                Console.WriteLine($"Name: {newOption?.Name}");
                Console.WriteLine($"Key: {newOption?.Key}");
                Console.WriteLine($"Value: {newOption?.Value}");
                Console.WriteLine($"Relation: {newOption?.Relation}");
                Console.WriteLine($"Order: {newOption?.Order}");
                Console.WriteLine($"IsSystem: {newOption?.IsSystem}");

                if (newOption == null || string.IsNullOrEmpty(newOption.Name))
                {
                    TempData["ErrorMessage"] = "Назва опції обов'язкова";
                    return RedirectToAction("Config");
                }

                // Якщо IsSystem не відмічено - встановлюємо false
                if (!newOption.IsSystem)
                {
                    newOption.IsSystem = false;
                }

                _agencyDBContext.Options.Add(newOption);
                _agencyDBContext.SaveChanges();

                TempData["SuccessMessage"] = "Нову опцію успішно додано!";
                return RedirectToAction("Config");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                TempData["ErrorMessage"] = "Помилка при додаванні опції: " + ex.Message;
                return RedirectToAction("Config");
            }
        }





        [HttpGet]
        public IActionResult GetRelations()
        {
            try
            {
                // Отримуємо унікальні значення Relation з бази даних
                var relations = _agencyDBContext.Options
                    .Select(o => o.Relation)
                    .Where(r => !string.IsNullOrEmpty(r))
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();

                Console.WriteLine($"✅ Found {relations.Count} unique relations");

                return Json(relations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting relations: {ex.Message}");
                return Json(new List<string>());
            }
        }







        //категорії 
        public IActionResult Categories()
        {
            var categories = _agencyDBContext.Categories?
                .Include(c => c.ParentCategory)
                .ToList();
            return View(categories);
        }

        public IActionResult EditCategory(int categoryId)
        {
            var category = _agencyDBContext.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefault(c => c.Id == categoryId);


            if (category == null)
            {
                return RedirectToAction("Categories");
            }

            ViewBag.AllCategories = _agencyDBContext.Categories
                .Where(c => c.Id != categoryId)
                .ToList();


            return View(category);

        }


        [HttpPost]
        public IActionResult UpdateCategory(Category category)
        {
            try
            {

                var existingCategory = _agencyDBContext.Categories
                    .FirstOrDefault(c => c.Id == category.Id);

                if (existingCategory == null)
                {

                    return RedirectToAction("Categories");
                }


                existingCategory.Name = category.Name ?? "";
                existingCategory.Slug = category.Slug ?? "";
                existingCategory.Description = category.Description ?? "";
                existingCategory.ImageSrc = category.ImageSrc ?? "";
                existingCategory.ParentID = category.ParentID;



                _agencyDBContext.SaveChanges();

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка при оновленні категорії: " + ex.Message;
            }

            return RedirectToAction("Categories");
        }





        [HttpGet]
        public IActionResult CreateCategory()
        {
            ViewBag.AllCategories = _agencyDBContext.Categories.ToList();
            return View();

        }

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            try
            {

                if (string.IsNullOrEmpty(category.Name))
                {
                    // Завантажуємо категорії для випадаючого списку
                    ViewBag.AllCategories = _agencyDBContext.Categories.ToList();
                    return View();
                }



                _agencyDBContext.Categories.Add(category);
                _agencyDBContext.SaveChanges();


            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка при створенні категорії: " + ex.Message;
                ViewBag.AllCategories = _agencyDBContext.Categories.ToList();
                return View(category);
            }

            return RedirectToAction("Categories");
        }




        [HttpGet]
        public JsonResult RemoveCategory(int categoryId)
        {
            try
            {
                var category = _agencyDBContext.Categories
                    .FirstOrDefault(c => c.Id == categoryId);

                if (category == null)
                {
                    return Json(new { success = false, message = "Категорія не знайдена." });
                }


                var childCategories = _agencyDBContext.Categories
                    .Where(c => c.ParentID == categoryId)
                    .ToList();

                if (childCategories.Any())
                {
                    return Json(new { success = false, message = "Спочатку видаліть або змініть батьківську категорію для підкатегорій." });
                }

                _agencyDBContext.Categories.Remove(category);
                _agencyDBContext.SaveChanges();

                return Json(new { success = true, message = "Категорія успішно видалена." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Помилка при видаленні категорії: " + ex.Message });
            }
        }





        public IActionResult Tags()
        {
            var tags = _agencyDBContext.Tags?.ToList();

            return View(tags);
        }

        [HttpGet]
        public IActionResult EditTag(int tagId)
        {
            var tag = _agencyDBContext.Tags
                .FirstOrDefault(t => t.Id == tagId);
            if (tag == null)
            {
                return RedirectToAction("Tags");
            }


            return View(tag);

        }



        [HttpPost]
        public IActionResult UpdateTag(Tag tag)
        {
            try
            {

                Console.WriteLine("=== UPDATE TAG ===");
                Console.WriteLine($"ID: {tag?.Id}");
                Console.WriteLine($"Name: {tag?.Name}");
                Console.WriteLine($"Slug: {tag?.Slug}");



                var existingTag = _agencyDBContext.Tags
                    .FirstOrDefault(t => t.Id == tag.Id);

                if (existingTag == null)
                {

                    return RedirectToAction("Tags");
                }

                var duplicateTag = _agencyDBContext.Tags
                    .FirstOrDefault(t => t.Name == tag.Name && t.Id != tag.Id);

                if (duplicateTag != null)
                {
                    return RedirectToAction("EditTag", new { tagId = tag.Id });
                }


                existingTag.Name = tag.Name?.Trim() ?? "";
                existingTag.Slug = tag.Slug?.Trim() ?? "";

                _agencyDBContext.SaveChanges();
                return RedirectToAction("Tags");
            }

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка при оновленні тегу: " + ex.Message;
                return RedirectToAction("EditTag", new { tagId = tag.Id });
            }




        }






        [HttpGet]
        public IActionResult CreateTag()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateTag(Tag tag)
        {
            try
            {

                if (string.IsNullOrEmpty(tag.Name))
                {
                    return View(tag);
                }

                var existingTag = _agencyDBContext.Tags
                    .FirstOrDefault(t => t.Name == tag.Name);

                if (existingTag != null)
                {
                    TempData["ErrorMessage"] = "Тег з такою назвою вже існує.";
                    return View(tag);
                }

                _agencyDBContext.Tags.Add(tag);
                _agencyDBContext.SaveChanges();

                return RedirectToAction("Tags");
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Помилка при створенні тегу: " + ex.Message;
                return View(tag);
            }

        }




        public JsonResult RemoveTag(int tagId)
        {
            try
            {
                var tag = _agencyDBContext.Tags
                    .FirstOrDefault(t => t.Id == tagId);

                if (tag == null)
                {
                    return Json(new { success = false, message = "Тег не знайдено." });
                }

                var postsWithTag = _agencyDBContext.PostTags.Any(pt => pt.TagId == tagId);
                if (postsWithTag)
                {
                    return Json(new { success = false, message = "Спочатку видаліть тег з усіх пов'язаних постів." });
                }

                _agencyDBContext.Tags.Remove(tag);
                _agencyDBContext.SaveChanges();

                return Json(new { success = true, message = "Тег успішно видалено." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Помилка при видаленні тегу: " + ex.Message });
            }

        }

    }

}