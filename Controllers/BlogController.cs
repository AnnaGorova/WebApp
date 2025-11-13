using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Db;
using WebApp.Entities;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class BlogController : Controller
    {
        private readonly AgencyDBContext _context;
        private readonly TagModel _tagModel;
        private const int PAGE_SIZE = 6; // Кількість постів на одній сторінці


        public BlogController(AgencyDBContext context, TagModel tagModel)
        {
            _context = context; // Доступ до бази даних
            _tagModel = tagModel; // Модель для роботи з тегами
        }



        // Головна сторінка блогу з пагінацією
        public IActionResult BlogGridIndex(int page = 1) // page = 1 - значення за замовчуванням
        {
            // Загальна кількість опублікованих постів для пагінації
            var totalPosts = _context.Posts
                .Where(p => p.PostStatuses == PostStatuses.Published)
                .Count();

            // Отримуємо пости для поточної сторінки
            var posts = _context.Posts
                .Where(p => p.PostStatuses == PostStatuses.Published) // Тільки опубліковані
                .Include(p => p.PostCategories) // Завантажуємо категорії
                .ThenInclude(pc => pc.Category) // Завантажуємо деталі категорій
                .Include(p => p.PostTags) // Завантажуємо теги
                .ThenInclude(pt => pt.Tag) // Завантажуємо деталі тегів
                .Include(p => p.Comments) // Завантажуємо коментарі
                .OrderByDescending(p => p.DataOfPublished) // Сортуємо за датою публікації (новіші перші)
                .Skip((page - 1) * PAGE_SIZE) // Пропускаємо пости попередніх сторінок
                .Take(PAGE_SIZE) // Беремо тільки PAGE_SIZE постів
                .ToList();

            // ПОПУЛЯРНІ КАТЕГОРІЇ (за кількістю постів)
            var popularCategories = _context.Categories
                .Where(c => c.PostCategories.Any()) // Тільки категорії, що мають пости
                .Select(c => new // Створюємо анонімний об'єкт
                {
                    Category = c, // Сама категорія
                    PostCount = c.PostCategories.Count() // Кількість постів у категорії
                })
                .OrderByDescending(x => x.PostCount) // Сортуємо за кількістю постів (популярніші перші)
                .Select(x => x.Category) // Вибераємо тільки категорії (без лічильника)
                .Take(5) // Беремо топ-5 категорій
                .ToList();

            // ПОПУЛЯРНІ ТЕГИ (за кількістю використань)
            var popularTags = _context.Tags
                .Where(tag => _context.PostTags.Any(pt => pt.TagId == tag.Id)) // Тільки теги, що використовуються
                .Select(tag => new // Створюємо анонімний об'єкт
                {
                    Tag = tag, // Сам тег
                    UsageCount = _context.PostTags.Count(pt => pt.TagId == tag.Id) // Кількість використань тега
                })
                .OrderByDescending(x => x.UsageCount) // Сортуємо за кількістю використань
                .Select(x => x.Tag) // Вибераємо тільки теги
                .Take(12) // Беремо топ-12 тегів
                .ToList();


            // Отримуємо Recent Posts (пости не з поточної сторінки)
            var currentPostIds = posts.Select(p => p.Id).ToList();
            var recentPosts = _context.Posts
                .Where(p => p.PostStatuses == PostStatuses.Published && !currentPostIds.Contains(p.Id))
                .OrderByDescending(p => p.DataOfPublished)
                .Take(5)
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
                .ToList();

            // Створюємо ViewModel для передачі даних у View
            var viewModel = new BlogViewModel
            {
                Posts = posts, // Пости поточної сторінки
                Categories = popularCategories, // Популярні категорії
                Tags = popularTags, // Популярні теги
                RecentPosts = recentPosts,
                CurrentPage = page, // Поточна сторінка
                TotalPages = (int)Math.Ceiling(totalPosts / (double)PAGE_SIZE) // Загальна кількість сторінок
            };

            return View(viewModel); // Повертаємо View з даними
        }

        // Детальна сторінка конкретного поста
        //public IActionResult BlogDetail(int id) // id поста
        //{
        //    // Знаходимо пост за ID з усіма включеними даними
        //    var post = _context.Posts
        //        .Include(p => p.PostCategories)
        //        .ThenInclude(pc => pc.Category)
        //        .Include(p => p.PostTags)
        //        .ThenInclude(pt => pt.Tag)
        //        .Include(p => p.Comments)
        //        .FirstOrDefault(p => p.Id == id);

        //    if (post == null) // Якщо пост не знайдено
        //    {
        //        return NotFound(); // Повертаємо 404 помилку
        //    }

        //    // Для детальної сторінки також використовуємо популярні категорії та теги
        //    var popularCategories = _context.Categories
        //        .Where(c => c.PostCategories.Any())
        //        .Select(c => new
        //        {
        //            Category = c,
        //            PostCount = c.PostCategories.Count()
        //        })
        //        .OrderByDescending(x => x.PostCount)
        //        .Select(x => x.Category)
        //        .Take(5)
        //        .ToList();

        //    var popularTags = _context.Tags
        //        .Where(tag => _context.PostTags.Any(pt => pt.TagId == tag.Id))
        //        .Select(tag => new
        //        {
        //            Tag = tag,
        //            UsageCount = _context.PostTags.Count(pt => pt.TagId == tag.Id)
        //        })
        //        .OrderByDescending(x => x.UsageCount)
        //        .Select(x => x.Tag)
        //        .Take(12)
        //        .ToList();

        //    var viewModel = new BlogViewModel
        //    {
        //        Posts = new List<Post> { post },  // Тільки один пост у списку
        //        Categories = popularCategories,
        //        Tags = popularTags
        //    };

        //    return View(viewModel);
        //}

        // метод для фільтрації по категорії
        public IActionResult Category(int categoryId, int page = 1)
        {
            var category = _context.Categories
                .FirstOrDefault(c => c.Id == categoryId);

            if (category == null) // Якщо категорія не знайдена
            {
                return NotFound();
            }

            // Загальна кількість постів у цій категорії
            var totalPosts = _context.Posts
                .Where(p => p.PostStatuses == PostStatuses.Published &&
                           p.PostCategories.Any(pc => pc.CategoryId == categoryId))  // Пости тільки з цієї категорії
                .Count();

            // Пости для поточної сторінки з фільтрацією по категорії
            var posts = _context.Posts
                .Where(p => p.PostStatuses == PostStatuses.Published &&
                           p.PostCategories.Any(pc => pc.CategoryId == categoryId)) // Фільтр по категорії
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.DataOfPublished)
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToList();

            // Популярні категорії для сайдбара
            var popularCategories = _context.Categories
                .Where(c => c.PostCategories.Any())
                .Select(c => new
                {
                    Category = c,
                    PostCount = c.PostCategories.Count()
                })
                .OrderByDescending(x => x.PostCount)
                .Select(x => x.Category)
                .Take(5)
                .ToList();
            
            // Популярні теги для сайдбара
            var popularTags = _context.Tags
                .Where(tag => _context.PostTags.Any(pt => pt.TagId == tag.Id))
                .Select(tag => new
                {
                    Tag = tag,
                    UsageCount = _context.PostTags.Count(pt => pt.TagId == tag.Id)
                })
                .OrderByDescending(x => x.UsageCount)
                .Select(x => x.Tag)
                .Take(5)
                .ToList();


            var recentPosts = _context.Posts
               .Where(p => p.PostStatuses == PostStatuses.Published)
               .OrderByDescending(p => p.DataOfPublished)
               .Take(5)
               .Include(p => p.PostCategories)
               .ThenInclude(pc => pc.Category)
               .Include(p => p.PostTags)
               .ThenInclude(pt => pt.Tag)
               .ToList();

            var viewModel = new BlogViewModel
            {
                Posts = posts,  // Відфільтровані пости
                Categories = popularCategories,
                Tags = popularTags,
                RecentPosts = recentPosts,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalPosts / (double)PAGE_SIZE),
                CurrentCategory = category.Name  // Назва поточної категорії для відображення
            };

            return View("BlogGridIndex", viewModel); // Використовуємо той самий View, але з іншими даними







            //public IActionResult BlogGridIndex()
            //{
            //    var viewModel = new BlogViewModel
            //    {
            //        Posts = _context.Posts
            //            .Where(p => p.PostStatuses == PostStatuses.Published)
            //            .Include(p => p.PostCategories)
            //            .ThenInclude(pc => pc.Category)
            //            .Include(p => p.PostTags)
            //            .ThenInclude(pt => pt.Tag)
            //            .Include(p => p.Comments)
            //            .OrderByDescending(p => p.DataOfPublished)
            //            .ToList(),

            //        Categories = _context.Categories
            //            .Where(c => c.PostCategories.Any())
            //            .ToList(),

            //        Tags = _tagModel.GetNotEmptyTags()
            //    };

            //    return View(viewModel);
            //}

            //public IActionResult BlogDetail(int id)
            //{
            //    var post = _context.Posts
            //        .Include(p => p.PostCategories)
            //        .ThenInclude(pc => pc.Category)
            //        .Include(p => p.PostTags)
            //        .ThenInclude(pt => pt.Tag)
            //        .Include(p => p.Comments)
            //        .FirstOrDefault(p => p.Id == id);

            //    if (post == null)
            //    {
            //        return NotFound();
            //    }

            //    var viewModel = new BlogViewModel
            //    {
            //        Posts = new List<Post> { post },
            //        Categories = _context.Categories
            //            .Where(c => c.PostCategories.Any())
            //            .ToList(),
            //        Tags = _tagModel.GetNotEmptyTags()
            //    };

            //    return View(viewModel);
            //}










        }




        // метод для фільтрації по тегу
        public IActionResult Tag(int tagId, int page = 1)
        {
            var tag = _context.Tags
                .FirstOrDefault(t => t.Id == tagId);

            if (tag == null) // Якщо тег не знайдений
            {
                return NotFound();
            }

            // Загальна кількість постів з цим тегом
            var totalPosts = _context.Posts
                .Where(p => p.PostStatuses == PostStatuses.Published &&
                           p.PostTags.Any(pt => pt.TagId == tagId)) // Пости тільки з цим тегом
                .Count();

            // Пости для поточної сторінки з фільтрацією по тегу
            var posts = _context.Posts
                .Where(p => p.PostStatuses == PostStatuses.Published &&
                           p.PostTags.Any(pt => pt.TagId == tagId)) // Фільтр по тегу
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.DataOfPublished)
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToList();

            // Популярні категорії для сайдбара
            var popularCategories = _context.Categories
                .Where(c => c.PostCategories.Any())
                .Select(c => new
                {
                    Category = c,
                    PostCount = c.PostCategories.Count()
                })
                .OrderByDescending(x => x.PostCount)
                .Select(x => x.Category)
                .Take(5)
                .ToList();

            // Популярні теги для сайдбара
            var popularTags = _context.Tags
                .Where(t => _context.PostTags.Any(pt => pt.TagId == t.Id))
                .Select(t => new
                {
                    Tag = t,
                    UsageCount = _context.PostTags.Count(pt => pt.TagId == t.Id)
                })
                .OrderByDescending(x => x.UsageCount)
                .Select(x => x.Tag)
                .Take(12)
                .ToList();


            var recentPosts = _context.Posts
               .Where(p => p.PostStatuses == PostStatuses.Published)
               .OrderByDescending(p => p.DataOfPublished)
               .Take(5)
               .Include(p => p.PostCategories)
               .ThenInclude(pc => pc.Category)
               .Include(p => p.PostTags)
               .ThenInclude(pt => pt.Tag)
               .ToList();


            var viewModel = new BlogViewModel
            {
                Posts = posts, // Відфільтровані пости
                Categories = popularCategories,
                Tags = popularTags,
                RecentPosts = recentPosts,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalPosts / (double)PAGE_SIZE),
                CurrentTag = tag.Name // Назва поточного тегу для відображення
            };

            return View("BlogGridIndex", viewModel); // Використовуємо той самий View
        }




        public IActionResult Search(string keyword, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return RedirectToAction("BlogGridIndex");
            }

            keyword = keyword.ToLower();

            var totalPosts = _context.Posts
                .Where(p => p.PostStatuses == PostStatuses.Published &&
                           (p.Name.ToLower().Contains(keyword) ||
                            p.Description.ToLower().Contains(keyword) ||
                            p.Context.ToLower().Contains(keyword) ||
                            p.PostTags.Any(pt => pt.Tag.Name.ToLower().Contains(keyword)) ||
                            p.PostCategories.Any(pc => pc.Category.Name.ToLower().Contains(keyword))))
                .Count();

            var posts = _context.Posts
                .Where(p => p.PostStatuses == PostStatuses.Published &&
                           (p.Name.ToLower().Contains(keyword) ||
                            p.Description.ToLower().Contains(keyword) ||
                            p.Context.ToLower().Contains(keyword) ||
                            p.PostTags.Any(pt => pt.Tag.Name.ToLower().Contains(keyword)) ||
                            p.PostCategories.Any(pc => pc.Category.Name.ToLower().Contains(keyword))))
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
                .OrderByDescending(p => p.DataOfPublished)
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToList();


           var popularCategories = _context.Categories
               .Where(c => c.PostCategories.Any())
               .Select(c => new
               {
                   Category = c,
                   PostCount = c.PostCategories.Count()
               })
               .OrderByDescending(x => x.PostCount)
               .Select(x => x.Category)
               .Take(5)
               .ToList();

            var popularTags = _context.Tags
                .Where(tag => _context.PostTags.Any(pt => pt.TagId == tag.Id))
                .Select(tag => new
                {
                    Tag = tag,
                    UsageCount = _context.PostTags.Count(pt => pt.TagId == tag.Id)
                })
                .OrderByDescending(x => x.UsageCount)
                .Select(x => x.Tag)
                .Take(12)
                .ToList();

            var recentPosts = _context.Posts
               .Where(p => p.PostStatuses == PostStatuses.Published)
               .OrderByDescending(p => p.DataOfPublished)
               .Take(5)
               .Include(p => p.PostCategories)
               .ThenInclude(pc => pc.Category)
               .Include(p => p.PostTags)
               .ThenInclude(pt => pt.Tag)
               .ToList();


            var viewModel = new BlogViewModel
            {
                Posts = posts,
                Categories = popularCategories,
                Tags = popularTags,
                RecentPosts = recentPosts,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalPosts / (double)PAGE_SIZE),
                CurrentCategory = $"Search results for: {keyword}"
            };

            return View("BlogGridIndex", viewModel);
        }



        public IActionResult BlogDetailIndex(int? id)
        {
            Post post = null;

            // СЦЕНАРІЙ 1: Якщо передано конкретний ID
            if (id.HasValue && id.Value > 0)
            {
                post = _context.Posts
                    .Include(p => p.PostCategories)
                    .ThenInclude(pc => pc.Category)
                    .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                    .Include(p => p.Comments)
                    .FirstOrDefault(p => p.Id == id.Value && p.PostStatuses == PostStatuses.Published);
            }

            // СЦЕНАРІЙ 2: Якщо пост не знайдено за ID або ID не передано
            if (post == null)
            {
                post = _context.Posts
                    .Where(p => p.PostStatuses == PostStatuses.Published)
                    .OrderByDescending(p => p.DataOfPublished)
                    .Include(p => p.PostCategories)
                    .ThenInclude(pc => pc.Category)
                    .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                    .Include(p => p.Comments)
                    .FirstOrDefault();

                if (post == null)
                {
                    return RedirectToAction("BlogGridIndex");
                }
            }

            // Отримання популярних категорій
            var popularCategories = _context.Categories
                .Where(c => c.PostCategories.Any())
                .Select(c => new
                {
                    Category = c,
                    PostCount = c.PostCategories.Count()
                })
                .OrderByDescending(x => x.PostCount)
                .Select(x => x.Category)
                .Take(5)
                .ToList();

            // Отримання популярних тегів
            var popularTags = _context.Tags
                .Where(tag => _context.PostTags.Any(pt => pt.TagId == tag.Id))
                .Select(tag => new
                {
                    Tag = tag,
                    UsageCount = _context.PostTags.Count(pt => pt.TagId == tag.Id)
                })
                .OrderByDescending(x => x.UsageCount)
                .Select(x => x.Tag)
                .Take(5)
                .ToList();

            // Отримання останніх постів (крім поточного)
            var recentPosts = _context.Posts
                .Where(p => p.PostStatuses == PostStatuses.Published && p.Id != post.Id)
                .OrderByDescending(p => p.DataOfPublished)
                .Take(5)
                .Include(p => p.PostCategories)
                .ThenInclude(pc => pc.Category)
                .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
                .ToList();

            var viewModel = new BlogViewModel
            {
                Posts = new List<Post> { post },
                Categories = popularCategories,
                Tags = popularTags,
                RecentPosts = recentPosts 
            };

            return View(viewModel);
        }

    }


}
