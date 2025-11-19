namespace WebApp.Dto
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string UserLogin { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime DateOfCreated { get; set; } = DateTime.Now;
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; } = null;
        public int Level { get; set; } = 0;
        public bool IsApproved { get; set; } = false; //  поле для модерації
    }
}
