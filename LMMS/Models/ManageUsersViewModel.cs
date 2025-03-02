using Microsoft.AspNetCore.Identity;

namespace LMMS.Models
{
    public class ManageUsersViewModel
    {
        // List of users to be displayed (either unconfirmed students or instructors)
        public List<IdentityUser> Users { get; set; } = new List<IdentityUser>();

        // List of available roles for assignment
        public List<string> Roles { get; set; }

        // Properties for handling the user actions
        public string UserId { get; set; }
        public string Role { get; set; }

        // For confirming email
        public string Action { get; set; } // "Confirm" or "Reject"

        // Confirmation token for email verification
        public string EmailConfirmationToken { get; set; }

        // This will hold users that should be shown as Students or Instructors in different tables
        public List<IdentityUser> Students { get; set; }
        public List<IdentityUser> Instructors { get; set; }
    }

}
