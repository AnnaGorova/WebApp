using WebApp.Entities;

namespace WebApp.ViewModels
{
    public class EditedViewOptionModel
    {
        public Option Option { get; set; }

        public  IEnumerable<string> Relations { get; set; } 
    }
}
