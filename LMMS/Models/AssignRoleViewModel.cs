using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace LMMS.Models
{
    public class AssignRoleViewModel
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public List<IdentityUser> Users { get; set; } = new List<IdentityUser>();
    }
}