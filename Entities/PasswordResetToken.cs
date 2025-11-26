using System.ComponentModel.DataAnnotations;

namespace WebApp.Entities
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}