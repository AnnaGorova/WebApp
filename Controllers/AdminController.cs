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





        public IActionResult Posts()
        {
            var posts = _agencyDBContext.Posts?.ToList();

            return View(posts);
        }

        //*********************************************************

        public IActionResult EditPost(int postId)
        {
            var post = _agencyDBContext.Posts
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.PostCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefault(p => p.Id == postId);

            if (post == null)
            {
                TempData["ErrorMessage"] = "Пост не знайдено";
                return RedirectToAction("Posts");
            }

            // Завантажуємо всі теги та категорії для вибору
            ViewBag.Tags = _agencyDBContext.Tags.ToList();
            ViewBag.Categories = _agencyDBContext.Categories.ToList();

            return View(post);
        }



        //[HttpGet]
        //public IActionResult EditPost(int postId)
        //{
        //    var post = _agencyDBContext.Posts
        //        .FirstOrDefault(p => p.Id == postId);
        //    if (post == null)
        //    {
        //        return RedirectToAction("Posts");
        //    }

        //    return View(post);
        //}
        //залиш закоментовано
        //[HttpPost]
        //public IActionResult UpdatePost(Post post)
        //{
        //    try
        //    {

        //        var existingPost = _agencyDBContext.Posts
        //            .FirstOrDefault(p => p.Id == post.Id);

        //        if (existingPost == null)
        //        {

        //            return RedirectToAction("Posts");
        //        }


        //        existingPost.Name = post.Name ?? "";
        //        existingPost.Slug = post.Slug ?? "";
        //        existingPost.Description = post.Description ?? "";
        //        existingPost.ImageSrc = post.ImageSrc ?? "";
        //        existingPost.Context = post.Context ?? "";
        //        existingPost.PostStatuses = post.PostStatuses;
        //        existingPost.DataOfUpdated = DateTime.Now;
        //        existingPost.DataOfPublished = post.DataOfPublished;
        //        existingPost.DataOfCreated = post.DataOfCreated;

        //        _agencyDBContext.SaveChanges();
        //        return RedirectToAction("Posts");

        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Помилка: " + ex.Message;
        //        return RedirectToAction("EditPost", new { postId = post.Id });
        //    }


        //}

        //[HttpPost]
        //public IActionResult UpdatePost(Post post, IFormFile ImageFile)
        //{
        //    try
        //    {
        //        var existingPost = _agencyDBContext.Posts
        //            .FirstOrDefault(p => p.Id == post.Id);

        //        if (existingPost == null)
        //        {
        //            TempData["ErrorMessage"] = "Пост не знайдено";
        //            return RedirectToAction("Posts");
        //        }

        //        // Оновлюємо поля
        //        existingPost.Name = post.Name ?? "";
        //        existingPost.Slug = post.Slug ?? "";
        //        existingPost.Description = post.Description ?? "";
        //        existingPost.Context = post.Context ?? "";
        //        existingPost.PostStatuses = post.PostStatuses;
        //        existingPost.DataOfUpdated = DateTime.Now;

        //        // Оновлюємо зображення, якщо завантажено нове
        //        if (ImageFile != null && ImageFile.Length > 0)
        //        {
        //            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
        //            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");

        //            if (!Directory.Exists(uploadsPath))
        //            {
        //                Directory.CreateDirectory(uploadsPath);
        //            }

        //            var filePath = Path.Combine(uploadsPath, fileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                ImageFile.CopyTo(stream);
        //            }

        //            existingPost.ImageSrc = "/img/" + fileName; // Правильний шлях
        //        }
        //        else if (!string.IsNullOrEmpty(post.ImageSrc))
        //        {
        //            // Якщо файл не завантажено, але є URL
        //            existingPost.ImageSrc = post.ImageSrc;
        //        }

        //        _agencyDBContext.SaveChanges();


        //        return RedirectToAction("Posts");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Помилка: " + ex.Message;
        //        return RedirectToAction("EditPost", new { postId = post.Id });
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePost(
            Post post,
            IFormFile? ImageFile = null,
            List<int>? SelectedTagIds = null,
            List<int>? SelectedCategoryIds = null)
        {
            try
            {
                // Знаходимо існуючий пост з залежностями
                var existingPost = _agencyDBContext.Posts
                    .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                    .Include(p => p.PostCategories)
                    .ThenInclude(pc => pc.Category)
                    .FirstOrDefault(p => p.Id == post.Id);

                if (existingPost == null)
                {
                    TempData["ErrorMessage"] = "Пост не знайдено";
                    return RedirectToAction("Posts");
                }

                // Дебаг логування
                Console.WriteLine($"=== ОНОВЛЕННЯ ПОСТУ ===");
                Console.WriteLine($"ID: {post.Id}");
                Console.WriteLine($"Назва: {post.Name}");
                Console.WriteLine($"Slug: {post.Slug}");
                Console.WriteLine($"Description: {post.Description}");
                Console.WriteLine($"ImageFile: {(ImageFile != null ? ImageFile.FileName : "null")}");
                Console.WriteLine($"ImageSrc: {post.ImageSrc}");
                Console.WriteLine($"PostStatuses: {post.PostStatuses}");
                Console.WriteLine($"SelectedTagIds: {(SelectedTagIds != null ? string.Join(", ", SelectedTagIds) : "null")}");
                Console.WriteLine($"SelectedCategoryIds: {(SelectedCategoryIds != null ? string.Join(", ", SelectedCategoryIds) : "null")}");
                Console.WriteLine($"=====================");

                // ОНОВЛЕННЯ ОСНОВНИХ ПОЛІВ
                existingPost.Name = post.Name;
                existingPost.Slug = post.Slug;
                existingPost.Description = post.Description ?? "";
                existingPost.Context = post.Context ?? "";
                existingPost.PostStatuses = post.PostStatuses;
                existingPost.DataOfUpdated = DateTime.Now;

                // ОНОВЛЕННЯ ЗОБРАЖЕННЯ
                // 1. Якщо завантажено новий файл
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    Console.WriteLine($"Обробка завантаженого файлу: {ImageFile.FileName}, Size: {ImageFile.Length} bytes");

                    // Перевірка типу файлу
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    var fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["ErrorMessage"] = "Дозволені лише файли зображень (JPG, PNG, GIF, BMP, WEBP)";
                        ViewBag.Tags = await _agencyDBContext.Tags.ToListAsync();
                        ViewBag.Categories = await _agencyDBContext.Categories.ToListAsync();
                        return View("EditPost", post);
                    }

                    // Перевірка розміру файлу (макс. 10MB)
                    if (ImageFile.Length > 10 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Розмір файлу не повинен перевищувати 10MB";
                        ViewBag.Tags = await _agencyDBContext.Tags.ToListAsync();
                        ViewBag.Categories = await _agencyDBContext.Categories.ToListAsync();
                        return View("EditPost", post);
                    }

                    // Створюємо папку для завантажень
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                        Console.WriteLine($"Створено папку: {uploadsFolder}");
                    }

                    // Генеруємо унікальне ім'я файлу
                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    Console.WriteLine($"Зберігаємо файл: {filePath}");

                    // Зберігаємо файл
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Оновлюємо шлях до зображення
                    existingPost.ImageSrc = "/uploads/" + fileName;
                    Console.WriteLine($"Оновлено ImageSrc: {existingPost.ImageSrc}");
                }
                // 2. Якщо не завантажено файл, але є URL в полі ImageSrc
                else if (!string.IsNullOrEmpty(post.ImageSrc))
                {
                    existingPost.ImageSrc = post.ImageSrc;
                    Console.WriteLine($"Використано URL з ImageSrc: {post.ImageSrc}");
                }
                // 3. Якщо ні файлу, ні URL - зберігаємо старе значення
                else
                {
                    Console.WriteLine($"Залишено старе зображення: {existingPost.ImageSrc}");
                }

                // ОНОВЛЕННЯ ТЕГІВ
                Console.WriteLine($"Оновлення тегів...");

                // Видаляємо всі існуючі теги поста
                if (existingPost.PostTags.Any())
                {
                    _agencyDBContext.PostTags.RemoveRange(existingPost.PostTags);
                    Console.WriteLine($"Видалено старих тегів: {existingPost.PostTags.Count}");
                }

                // Додаємо нові теги, якщо вони обрані
                if (SelectedTagIds != null && SelectedTagIds.Any())
                {
                    Console.WriteLine($"Додаємо нові теги: {string.Join(", ", SelectedTagIds)}");

                    foreach (var tagId in SelectedTagIds)
                    {
                        // Перевіряємо, чи існує такий тег
                        var tagExists = await _agencyDBContext.Tags.AnyAsync(t => t.Id == tagId);
                        if (tagExists)
                        {
                            var postTag = new PostTags
                            {
                                PostId = existingPost.Id,
                                TagId = tagId
                            };
                            _agencyDBContext.PostTags.Add(postTag);
                            Console.WriteLine($"Додано тег з ID: {tagId}");
                        }
                        else
                        {
                            Console.WriteLine($"Тег з ID {tagId} не знайдено в базі даних");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Теги не обрані");
                }

                // ОНОВЛЕННЯ КАТЕГОРІЙ
                Console.WriteLine($"Оновлення категорій...");

                // Видаляємо всі існуючі категорії поста
                if (existingPost.PostCategories.Any())
                {
                    _agencyDBContext.PostCategories.RemoveRange(existingPost.PostCategories);
                    Console.WriteLine($"Видалено старих категорій: {existingPost.PostCategories.Count}");
                }

                // Додаємо нові категорії, якщо вони обрані
                if (SelectedCategoryIds != null && SelectedCategoryIds.Any())
                {
                    Console.WriteLine($"Додаємо нові категорії: {string.Join(", ", SelectedCategoryIds)}");

                    foreach (var categoryId in SelectedCategoryIds)
                    {
                        // Перевіряємо, чи існує така категорія
                        var categoryExists = await _agencyDBContext.Categories.AnyAsync(c => c.Id == categoryId);
                        if (categoryExists)
                        {
                            var postCategory = new PostCategories
                            {
                                PostId = existingPost.Id,
                                CategoryId = categoryId
                            };
                            _agencyDBContext.PostCategories.Add(postCategory);
                            Console.WriteLine($"Додано категорію з ID: {categoryId}");
                        }
                        else
                        {
                            Console.WriteLine($"Категорія з ID {categoryId} не знайдено в базі даних");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Категорії не обрані");
                }

                // ОНОВЛЕННЯ ДАТИ ПУБЛІКАЦІЇ
                // Якщо пост став опублікованим і дата публікації ще не встановлена
                if (existingPost.PostStatuses == PostStatuses.Published &&
                    existingPost.DataOfPublished == default)
                {
                    existingPost.DataOfPublished = DateTime.Now;
                    Console.WriteLine($"Встановлено дату публікації: {existingPost.DataOfPublished}");
                }

                // ЗБЕРЕЖЕННЯ ЗМІН
                Console.WriteLine($"Зберігаємо зміни в базі даних...");
                await _agencyDBContext.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Пост '{post.Name}' успішно оновлено!";
                return RedirectToAction("EditPost", new { postId = post.Id });
            }
            catch (DbUpdateException dbEx)
            {
                // Обробка помилок бази даних
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                Console.WriteLine($"Помилка бази даних: {errorMessage}");
                TempData["ErrorMessage"] = $"Помилка бази даних: {errorMessage}";

                // Завантажуємо теги та категорії для повторного відображення форми
                ViewBag.Tags = await _agencyDBContext.Tags.ToListAsync();
                ViewBag.Categories = await _agencyDBContext.Categories.ToListAsync();

                return View("EditPost", post);
            }
            catch (IOException ioEx)
            {
                // Обробка помилок файлової системи
                Console.WriteLine($"Помилка файлової системи: {ioEx.Message}");
                TempData["ErrorMessage"] = $"Помилка збереження файлу: {ioEx.Message}";

                ViewBag.Tags = await _agencyDBContext.Tags.ToListAsync();
                ViewBag.Categories = await _agencyDBContext.Categories.ToListAsync();

                return View("EditPost", post);
            }
            catch (Exception ex)
            {
                // Загальна обробка помилок
                Console.WriteLine($"Загальна помилка: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Помилка: {ex.Message}";

                // Завантажуємо теги та категорії для повторного відображення форми
                ViewBag.Tags = await _agencyDBContext.Tags.ToListAsync();
                ViewBag.Categories = await _agencyDBContext.Categories.ToListAsync();

                return View("EditPost", post);
            }
        }






        [HttpGet]
        public IActionResult CreatePost()
        {
            // Отримуємо категорії та теги для випадаючих списків
            var categories = _agencyDBContext.Categories?.ToList() ?? new List<Category>();
            var tags = _agencyDBContext.Tags?.ToList() ?? new List<Tag>();

            ViewData["Categories"] = categories;
            ViewData["Tags"] = tags;

            return View(new Post());
        }

        [HttpPost]
        public IActionResult CreatePost(Post post, IFormFile ImageFile, List<int> SelectedCategoryIds, List<int> SelectedTagIds)
        {
            try
            {
                // Валідація обов'язкових полів
                if (string.IsNullOrEmpty(post.Name))
                {
                    TempData["ErrorMessage"] = "Назва посту обов'язкова";
                    return RedirectToAction("CreatePost");
                }

                if (string.IsNullOrEmpty(post.Slug))
                {
                    TempData["ErrorMessage"] = "Slug обов'язковий";
                    return RedirectToAction("CreatePost");
                }

                // Перевірка унікальності слага
                var existingPost = _agencyDBContext.Posts
                    .FirstOrDefault(p => p.Slug == post.Slug);

                if (existingPost != null)
                {
                    TempData["ErrorMessage"] = "Пост з таким Slug вже існує";
                    return RedirectToAction("CreatePost");
                }

                // Перевірка категорій
                if (SelectedCategoryIds == null || !SelectedCategoryIds.Any())
                {
                    TempData["ErrorMessage"] = "Виберіть хоча б одну категорію";
                    return RedirectToAction("CreatePost");
                }

                // Обробка зображення
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");

                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageFile.CopyTo(stream);
                    }

                    post.ImageSrc = "/img/" + fileName;
                }
                else if (string.IsNullOrEmpty(post.ImageSrc))
                {
                    post.ImageSrc = "/img/default-post.jpg"; // Дефолтне зображення
                }

                // Встановлення дат
                post.DataOfCreated = DateTime.Now;
                post.DataOfUpdated = DateTime.Now;

                if (post.PostStatuses == PostStatuses.Published)
                {
                    post.DataOfPublished = DateTime.Now;
                }

                // Створення посту (спочатку без зв'язків)
                _agencyDBContext.Posts?.Add(post);
                _agencyDBContext.SaveChanges(); // Зберігаємо, щоб отримати Id

                // Додавання зв'язків з категоріями через проміжну таблицю
                foreach (var categoryId in SelectedCategoryIds)
                {
                    var categoryExists = _agencyDBContext.Categories?.Any(c => c.Id == categoryId) ?? false;
                    if (categoryExists)
                    {
                        var postCategory = new PostCategories
                        {
                            PostId = post.Id,
                            CategoryId = categoryId
                        };
                        _agencyDBContext.PostCategories?.Add(postCategory);
                    }
                }

                // Додавання зв'язків з тегами через проміжну таблицю
                if (SelectedTagIds != null && SelectedTagIds.Any())
                {
                    foreach (var tagId in SelectedTagIds)
                    {
                        var tagExists = _agencyDBContext.Tags?.Any(t => t.Id == tagId) ?? false;
                        if (tagExists)
                        {
                            var postTag = new PostTags
                            {
                                PostId = post.Id,
                                TagId = tagId
                            };
                            _agencyDBContext.PostTags?.Add(postTag);
                        }
                    }
                }

                // Зберігаємо всі зв'язки
                _agencyDBContext.SaveChanges();

                
                return RedirectToAction("Posts");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка при створенні посту: " + ex.Message;

                // Повертаємо категорії та теги для повторного відображення
                var categories = _agencyDBContext.Categories?.ToList() ?? new List<Category>();
                var tags = _agencyDBContext.Tags?.ToList() ?? new List<Tag>();

                ViewData["Categories"] = categories;
                ViewData["Tags"] = tags;

                return View(post);
            }
        }







        [HttpPost]
        public IActionResult DeletePost(int postId)
        {
            try
            {
                var post = _agencyDBContext.Posts?.FirstOrDefault(p => p.Id == postId);

                if (post == null)
                {
                    return Json(new { success = false, message = "Пост не знайдено" });
                }

                // Видаляємо зв'язки з категоріями
                var postCategories = _agencyDBContext.PostCategories?.Where(pc => pc.PostId == postId).ToList();
                if (postCategories != null && postCategories.Any())
                {
                    _agencyDBContext.PostCategories?.RemoveRange(postCategories);
                }

                // Видаляємо зв'язки з тегами
                var postTags = _agencyDBContext.PostTags?.Where(pt => pt.PostId == postId).ToList();
                if (postTags != null && postTags.Any())
                {
                    _agencyDBContext.PostTags?.RemoveRange(postTags);
                }

                // Видаляємо коментарі (якщо є таблиця коментарів)
                var comments = _agencyDBContext.Comments?.Where(c => c.PostId == postId).ToList();
                if (comments != null && comments.Any())
                {
                    _agencyDBContext.Comments?.RemoveRange(comments);
                }

                // Видаляємо сам пост
                _agencyDBContext.Posts?.Remove(post);
                _agencyDBContext.SaveChanges();

                return Json(new { success = true, message = "Пост успішно видалено" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Помилка при видаленні: {ex.Message}" });
            }
        }





        //public IActionResult Navigation()
        //{
        //    // Отримуємо всі пункти меню з бази даних
        //    var allNavigates = _agencyDBContext.Navigates
        //        .Include(n => n.Childs)
        //        .OrderBy(n => n.Order)
        //        .ToList();

        //    // Створюємо список для відображення (без дублікатів)
        //    var displayList = new List<Navigate>();

        //    // Додаємо всі елементи
        //    foreach (var navigate in allNavigates)
        //    {
        //        // Додаємо поточний елемент
        //        displayList.Add(navigate);

        //        // Додаємо всіх нащадків
        //        AddAllDescendants(navigate, displayList);
        //    }

        //    // Видаляємо дублікати (якщо є)
        //    displayList = displayList.Distinct().OrderBy(n => n.Order).ToList();

        //    // Зберігаємо всі навігації в ViewBag для пошуку батьків
        //    ViewBag.AllNavigates = allNavigates;

        //    return View(displayList);
        //}

        //// Допоміжний метод для додавання всіх нащадків
        //private void AddAllDescendants(Navigate parent, List<Navigate> list)
        //{
        //    if (parent.Childs?.Any() == true)
        //    {
        //        foreach (var child in parent.Childs.OrderBy(c => c.Order))
        //        {
        //            list.Add(child);
        //            AddAllDescendants(child, list);
        //        }
        //    }
        //}

        public IActionResult Navigation()
        {
            // 1. Отримуємо всі пункти меню
            var allNavigates = _agencyDBContext.Navigates
                .Include(n => n.Childs)
                .ToList();

            // 2. Створюємо відсортований список: спочатку батьки, потім діти
            var displayList = new List<Navigate>();

            // Спочатку всі батьки (відсортовані)
            var parents = allNavigates
                .Where(n => n.ParentID == null)
                .OrderBy(n => n.Order)
                .ThenBy(n => n.Id);

            foreach (var parent in parents)
            {
                // Додаємо батька
                displayList.Add(parent);

                // Додаємо всіх його дітей (відсортованих)
                var children = allNavigates
                    .Where(n => n.ParentID == parent.Id)
                    .OrderBy(n => n.Order)
                    .ThenBy(n => n.Id);

                displayList.AddRange(children);
            }

            // 3. Для тих, хто не має батька але не кореневий (на всяк випадок)
            var orphans = allNavigates
                .Where(n => n.ParentID != null && !allNavigates.Any(p => p.Id == n.ParentID))
                .OrderBy(n => n.Order)
                .ThenBy(n => n.Id);

            displayList.AddRange(orphans);

            // 4. Зберігаємо всі навігації в ViewBag
            ViewBag.AllNavigates = allNavigates;

            return View(displayList);
        }






        [HttpGet]
        public IActionResult EditNavigation(int id)
        {
            var navigate = _agencyDBContext.Navigates
                .Include(n => n.Childs)
                .FirstOrDefault(n => n.Id == id);

            if (navigate == null)
            {
                TempData["ErrorMessage"] = "Пункт меню не знайдено";
                return RedirectToAction("Navigation");
            }

            // Отримуємо всі навігації для вибору батька
            var allNavigates = _agencyDBContext.Navigates
                .Where(n => n.Id != id) // Виключаємо поточний елемент
                .ToList();

            ViewBag.AllNavigates = allNavigates;
            return View(navigate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateNavigation(Navigate navigate)
        {
            try
            {
                var existingNavigate = _agencyDBContext.Navigates.Find(navigate.Id);

                if (existingNavigate == null)
                {
                    TempData["ErrorMessage"] = "Пункт меню не знайдено";
                    return RedirectToAction("Navigation");
                }

                // Оновлюємо поля
                existingNavigate.Title = navigate.Title;
                existingNavigate.Href = string.IsNullOrEmpty(navigate.Href) ? "#" : navigate.Href;
                existingNavigate.Order = navigate.Order;
                existingNavigate.ParentID = navigate.ParentID;

                _agencyDBContext.SaveChanges();
                TempData["SuccessMessage"] = "Пункт меню успішно оновлено!";

                return RedirectToAction("Navigation");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка при оновленні пункту меню: " + ex.Message;
                return RedirectToAction("EditNavigation", new { id = navigate.Id });
            }
        }





        [HttpGet]
        public IActionResult CreateNavigation()
        {
            var allNavigates = _agencyDBContext.Navigates.ToList();

            // Автоматично заповнюємо наступний порядок
            int nextOrder = _agencyDBContext.Navigates
                .Where(n => n.ParentID == null) // для кореневих
                .Max(n => (int?)n.Order) ?? 0;

            ViewBag.NextOrder = nextOrder + 1;
            ViewBag.AllNavigates = allNavigates;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNavigation(Navigate navigate)
        {
            try
            {
                navigate.Href = string.IsNullOrEmpty(navigate.Href) ? "#" : navigate.Href;

                // Якщо не вказано порядок або 0 - ставимо автоматично
                if (navigate.Order <= 0)
                {
                    var maxOrder = _agencyDBContext.Navigates
                        .Where(n => n.ParentID == navigate.ParentID)
                        .Max(n => (int?)n.Order) ?? 0;

                    navigate.Order = maxOrder + 1;
                }
                // Якщо вказаний порядок зайнятий - ЗСУВАЄМО
                else if (_agencyDBContext.Navigates
                    .Any(n => n.ParentID == navigate.ParentID && n.Order == navigate.Order))
                {
                    // Зсуваємо всі після цього порядку
                    var itemsToShift = _agencyDBContext.Navigates
                        .Where(n => n.ParentID == navigate.ParentID && n.Order >= navigate.Order)
                        .OrderByDescending(n => n.Order)
                        .ToList();

                    foreach (var item in itemsToShift)
                    {
                        item.Order++;
                    }
                }

                _agencyDBContext.Navigates.Add(navigate);
                _agencyDBContext.SaveChanges();

                TempData["SuccessMessage"] = $"Пункт '{navigate.Title}' створено (порядок: {navigate.Order})!";
                return RedirectToAction("Navigation");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Помилка: " + ex.Message;
                var allNavigates = _agencyDBContext.Navigates.ToList();
                ViewBag.AllNavigates = allNavigates;
                return View(navigate);
            }
        }



        [HttpGet]
        public IActionResult DeleteNavigation(int id)
        {
            try
            {
                // Знаходимо пункт з усіма дочірніми елементами
                var navigate = _agencyDBContext.Navigates
                    .Include(n => n.Childs)
                    .FirstOrDefault(n => n.Id == id);

                if (navigate == null)
                {
                    return Json(new { success = false, message = "Пункт меню не знайдено" });
                }

                string title = navigate.Title;
                int childrenCount = navigate.Childs?.Count ?? 0;

                // Рекурсивно видаляємо всіх нащадків
                DeleteNavigationRecursive(navigate);

                // Видаляємо сам пункт
                _agencyDBContext.Navigates.Remove(navigate);
                _agencyDBContext.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"Пункт '{title}' успішно видалено" +
                              (childrenCount > 0 ? $" (разом з {childrenCount} дочірніми)" : "")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Помилка при видаленні: " + ex.Message
                });
            }
        }

        // Рекурсивний метод для видалення всіх нащадків
        private void DeleteNavigationRecursive(Navigate parent)
        {
            if (parent.Childs?.Any() == true)
            {
                var children = parent.Childs.ToList();
                foreach (var child in children)
                {
                    DeleteNavigationRecursive(child);
                    _agencyDBContext.Navigates.Remove(child);
                }
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult DeleteNavigation(int navigationId)
        //{
        //    try
        //    {
        //        // Знаходимо пункт з усіма дочірніми елементами
        //        var navigate = _agencyDBContext.Navigates
        //            .Include(n => n.Childs)
        //            .FirstOrDefault(n => n.Id == navigationId);

        //        if (navigate == null)
        //        {
        //            return Json(new { success = false, message = "Пункт меню не знайдено" });
        //        }

        //        // Рекурсивно видаляємо всіх нащадків
        //        DeleteNavigationRecursive(navigate);

        //        // Видаляємо сам пункт
        //        _agencyDBContext.Navigates.Remove(navigate);
        //        _agencyDBContext.SaveChanges();

        //        return Json(new
        //        {
        //            success = true,
        //            message = $"Пункт '{navigate.Title}' успішно видалено"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Помилка при видаленні: " + ex.Message
        //        });
        //    }
        //}

        // Рекурсивний метод для видалення всіх нащадків
        //private void DeleteNavigationRecursive(Navigate parent)
        //{
        //    if (parent.Childs?.Any() == true)
        //    {
        //        foreach (var child in parent.Childs.ToList()) // ToList() щоб уникнути змін під час ітерації
        //        {
        //            DeleteNavigationRecursive(child);
        //            _agencyDBContext.Navigates.Remove(child);
        //        }
        //    }
        //}

    }
}
