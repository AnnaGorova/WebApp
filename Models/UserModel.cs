using Microsoft.AspNetCore.Identity;
using WebApp.Db;
using WebApp.Entities;

namespace WebApp.Models
{
    public class UserModel
    {
        private AgencyDBContext _agencyDBContext;
        public UserModel(AgencyDBContext agencyDBContext)
        {
            _agencyDBContext = agencyDBContext;
        }

        
        public User? GetUserByEmail(string email)
        {
            return _agencyDBContext.Users.SingleOrDefault(u => u.Email.ToLower() == email.ToLower());

        }
    }
}
