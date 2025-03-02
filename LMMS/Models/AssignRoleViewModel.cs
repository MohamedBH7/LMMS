using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace LMMS.Models
{
    public class AssignRoleViewModel
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public List<IdentityUser> Users { get; set; } = new List<IdentityUser>();
        public List<IdentityUser> Students { get; internal set; }
        public List<IdentityUser> Instructors { get; internal set; }
        public List<string> Roles { get; set; } = new List<string>();


        public List<UserWithRolesViewModel> UsersWithRoles { get; set; } = new List<UserWithRolesViewModel>();

    }
}