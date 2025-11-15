namespace WebApp.Dto
{
    public class CommentRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public string PostSlug { get; set; } = string.Empty;
    }
}
