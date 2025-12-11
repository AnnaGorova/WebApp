using WebApp.Entities;

namespace WebApp.ViewModels
{
    public class UserProfileViewModel
    {
        public string Email { get; set; }
        public string Login { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string GoogleId { get; set; }
        public AuthType AuthType { get; set; }
    }
}
