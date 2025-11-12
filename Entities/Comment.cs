using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string UserLogin { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        public DateTime DateOfCreated { get; set; } = DateTime.Now;
        public bool IsRequired { get; set; } = false;

        public int PostId { get; set; } 
        public int? ParentCommentId { get; set; } = null;
        public virtual Post Post { get; set; }

        public virtual Comment ParentComment { get; set; }
        public virtual ICollection<Comment> Childs { get; set; } = new List<Comment>();
    }
}
