using Microsoft.EntityFrameworkCore;
using WebApp.Db;
using WebApp.Entities;

namespace WebApp.Models
{
    public class CommentsModel
    {
        private AgencyDBContext _agencyDBContext;

        public CommentsModel(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
        }

        // Отримати всі коментарі для поста з правильною рекурсивною структурою
        public IEnumerable<Comment> GetCommentsTree(int postId)
        {
            var allComments = _agencyDBContext.Comments
                .Include(c => c.Childs) // включаємо дочірні коментарі
                .Where(c => c.PostId == postId)
                .ToList();

            return BuildCommentsTree(allComments, null);
        }



        public IEnumerable<Comment> GetApprovedCommentsTree(int postId)
        {
            var allComments = _agencyDBContext.Comments
                .Include(c => c.Childs)
                .Where(c => c.PostId == postId && c.IsApproved == true) // ✅ Тільки схвалені
                .ToList();

            return BuildCommentsTree(allComments, null);
        }



        // Рекурсивний метод для побудови дерева коментарів (аналогічно навігації)
        private List<Comment> BuildCommentsTree(List<Comment> allComments, int? parentCommentId)
        {
            return allComments
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.DateOfCreated)
                .Select(c => new Comment
                {
                    Id = c.Id,
                    UserLogin = c.UserLogin,
                    UserEmail = c.UserEmail,
                    UserAvatar = c.UserAvatar,
                    Text = c.Text,
                    DateOfCreated = c.DateOfCreated,
                    PostId = c.PostId,
                    ParentCommentId = c.ParentCommentId,
                    IsRequired = c.IsRequired,
                    Childs = BuildCommentsTree(allComments, c.Id) // ⭐ РЕКУРСІЯ ⭐
                })
                .ToList();
        }

        // Старий метод для зворотної сумісності
        public List<Comment> GetAllComments(int postId)
        {
            return _agencyDBContext.Comments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.DateOfCreated)
                .ToList();
        }
    }
}