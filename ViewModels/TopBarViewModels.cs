using WebApp.Entities;

namespace WebApp.ViewModels
{
    public class TopBarViewModels
    {
        public IEnumerable<Option> SocialLinks { get; set; } = new List<Option>(); 
        public IEnumerable<Option> ContactInformation { get; set; } = new List<Option>();  
    }
}
