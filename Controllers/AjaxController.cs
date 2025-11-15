using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApp.Db;
using WebApp.Dto;
using WebApp.Entities;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class AjaxController : Controller
    {
        private readonly AgencyDBContext _agencyDBContext;

        private PostModel _postModel;
        private CommentsModel _commentsModel;

        public AjaxController(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
            _postModel = new PostModel(agencyDBContext);
            _commentsModel = new CommentsModel(agencyDBContext);
        }




        // "Коли хтось питає про коментарі - я їх знайду і відправлю!"
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public string GetAllComments(string slug)
        {
            JsonResponse jsonResponseError = new JsonResponse()
            {
                Code = 400,
                Status = StatusResponse.Error,
                Message = "Slug not valid data",
                Data = null
            };

            if (slug == null)
            {
                return JsonSerializer.Serialize(jsonResponseError);
            }

            Post? post = _postModel.GetPostBySlug(slug);
            if (post == null)
            {
                jsonResponseError.Message = "Post Not found";
                return JsonSerializer.Serialize(jsonResponseError);
            }

            Console.WriteLine($"🔍 Getting comments for post: {post.Name} (ID: {post.Id})");

            // ✅ ВИКОРИСТОВУЄМО ДЕРЕВОПОДІБНИЙ МЕТОД ЗАМІСТЬ ПЛОСКОГО СПИСКУ
            var commentsTree = _commentsModel.GetCommentsTree(post.Id);
            

            // Конвертуємо дерево в плоский список з рівнями для відображення
            List<CommentDto> commentDtos = ConvertTreeToFlatList(commentsTree);
          

            JsonSerializerOptions jso = new JsonSerializerOptions();
            jso.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            JsonResponse response = new JsonResponse()
            {
                Code = 200,
                Status = StatusResponse.Success,
                Message = "Data loaded",
                Data = JsonSerializer.Serialize(commentDtos, jso)
            };

            return JsonSerializer.Serialize(response, jso);
        }





        private List<CommentDto> ConvertTreeToFlatList(IEnumerable<Comment> comments, int level = 0)
        {
            var result = new List<CommentDto>();

            foreach (var comment in comments)
            {
                result.Add(new CommentDto()
                {
                    Id = comment.Id,
                    UserLogin = comment.UserLogin,
                    UserEmail = comment.UserEmail,
                    UserAvatar = comment.UserAvatar,
                    Text = comment.Text,
                    DateOfCreated = comment.DateOfCreated,
                    PostId = comment.PostId,
                    ParentCommentId = comment.ParentCommentId,
                    Level = level // Додаємо інформацію про рівень вкладеності
                });

                // Рекурсивно додаємо дочірні коментарі
                if (comment.Childs != null && comment.Childs.Any())
                {
                    result.AddRange(ConvertTreeToFlatList(comment.Childs, level + 1));
                }
            }

            return result;
        }








        //  https://localhost:7235/Ajax/GetAllComments?slug=getting-started-aspnet-core
        [HttpGet]
        [Route("/test-slugs")]
        public IActionResult TestAllSlugs()
        {
            var posts = _agencyDBContext.Posts.ToList();

            var result = new StringBuilder();
            result.AppendLine("<h1>All Available Slugs</h1>");
            result.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%;'>");
            result.AppendLine("<tr style='background-color: #f0f0f0;'><th>ID</th><th>Name</th><th>Slug</th><th>Status</th><th>Test Link</th></tr>");

            foreach (var post in posts)
            {
                result.AppendLine($"<tr>");
                result.AppendLine($"<td style='padding: 8px;'>{post.Id}</td>");
                result.AppendLine($"<td style='padding: 8px;'>{post.Name}</td>");
                result.AppendLine($"<td style='padding: 8px;'><strong>{post.Slug}</strong></td>");
                result.AppendLine($"<td style='padding: 8px;'>{post.PostStatuses}</td>");
                result.AppendLine($"<td style='padding: 8px;'><a href='/Ajax/GetAllComments?slug={post.Slug}' target='_blank'>Test Comments</a></td>");
                result.AppendLine($"</tr>");
            }

            result.AppendLine("</table>");
            return Content(result.ToString(), "text/html");
        }




        // https://localhost:7235/check-comments/1
        [HttpGet]
        [Route("/check-comments/{postId}")]
        public IActionResult CheckComments(int postId)
        {
            var result = new StringBuilder();
            result.AppendLine($"<h1>Checking Comments for Post ID: {postId}</h1>");

            // Перевіряємо всі коментарі
            var allComments = _agencyDBContext.Comments
                .Where(c => c.PostId == postId)
                .ToList();

            result.AppendLine($"<p>Total comments for post: {allComments.Count}</p>");

            // Перевіряємо коментарі з IsRequired = true
            var requiredComments = _agencyDBContext.Comments
                .Where(c => c.PostId == postId && c.IsRequired == true)
                .ToList();

            result.AppendLine($"<p>Comments with IsRequired = true: {requiredComments.Count}</p>");

            result.AppendLine("<h3>All Comments:</h3>");
            result.AppendLine("<table border='1'>");
            result.AppendLine("<tr><th>ID</th><th>User</th><th>Text</th><th>IsRequired</th></tr>");

            foreach (var comment in allComments)
            {
                result.AppendLine($"<tr>");
                result.AppendLine($"<td>{comment.Id}</td>");
                result.AppendLine($"<td>{comment.UserLogin}</td>");
                result.AppendLine($"<td>{comment.Text}</td>");
                result.AppendLine($"<td>{comment.IsRequired}</td>");
                result.AppendLine($"</tr>");
            }

            result.AppendLine("</table>");

            return Content(result.ToString(), "text/html");
        }









        // "Коли хтось надсилає новий коментар - я його збережу!"
        [HttpPost]
        public string AddComment([FromBody] CommentRequest request)
        {
            try
            {
                Console.WriteLine($"📨 AddComment called for post: {request.PostSlug}");

                // Валідація
                if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.CommentText))
                {
                    return JsonSerializer.Serialize(new JsonResponse()
                    {
                        Code = 400,
                        Status = StatusResponse.Error,
                        Message = "Name and comment text are required",
                        Data = null
                    });
                }

                // Знаходимо пост
                var post = _postModel.GetPostBySlug(request.PostSlug);
                if (post == null)
                {
                    return JsonSerializer.Serialize(new JsonResponse()
                    {
                        Code = 404,
                        Status = StatusResponse.Error,
                        Message = "Post not found",
                        Data = null
                    });
                }

                // Створюємо новий коментар
                var newComment = new Comment
                {
                    UserLogin = request.UserName,
                    UserEmail = request.UserEmail ?? "",
                    UserAvatar = "user.jpg", // дефолтне зображення
                    Text = request.CommentText,
                    DateOfCreated = DateTime.Now,
                    PostId = post.Id,
                    ParentCommentId = request.ParentCommentId,
                    IsRequired = true
                };

                // Зберігаємо в базу
                _agencyDBContext.Comments.Add(newComment);
                _agencyDBContext.SaveChanges();

                Console.WriteLine($"✅ Comment added successfully! ID: {newComment.Id}");

                return JsonSerializer.Serialize(new JsonResponse()
                {
                    Code = 200,
                    Status = StatusResponse.Success,
                    Message = "Comment added successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding comment: {ex}");
                return JsonSerializer.Serialize(new JsonResponse()
                {
                    Code = 500,
                    Status = StatusResponse.Error,
                    Message = "Server error: " + ex.Message,
                    Data = null
                });
            }
        }

        









    }
}
