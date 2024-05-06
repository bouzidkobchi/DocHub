using DocHub.Api.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace DocHub.Api.Models
{
    public class UserDTO(AppUser user /*string[] UsersRoles*/)
    {
        public string FullName { get; private set; } = user.UserName;
        public string Email { get; set; } = user.Email;
        public string Id { get; set; } = user.Id;
        //public string[] Roles { get; set; } = UsersRoles;
    }
}
