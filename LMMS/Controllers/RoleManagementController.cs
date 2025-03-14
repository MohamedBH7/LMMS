using LMMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMMS.Controllers
{
    public class RoleManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public RoleManagementController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // Display the form
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AssignRole()
        { 
            var model = new AssignRoleViewModel();

            // Get the list of users
            var users = _userManager.Users.ToList();

            // For each user, get the associated roles
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);  // Fetch roles for the user
                var userWithRoles = new UserWithRolesViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = roles.ToList()  // Add roles to the user
                };
                if(roles.Count != 0)
                {

                model.UsersWithRoles.Add(userWithRoles);

                }
                model.Users.Add(user);
            }

            // You can also populate the list of available roles in case you need it for the dropdown.
          //  model.Roles = await _context.Roles.Select(r => r.Name).ToListAsync();  // Fetch roles from database (if needed)

            return View(model);
        }



        [Authorize(Roles = "Admin")]

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Manageuser()
        {
            var users = _userManager.Users.ToList();

            var students = users.Where(u => u.Email.StartsWith("bh", StringComparison.OrdinalIgnoreCase)).ToList();
            var instructors = users.Where(u => !u.Email.StartsWith("bh", StringComparison.OrdinalIgnoreCase)).ToList();
            var _Users = users.Where(u => !u.EmailConfirmed).ToList();

            var model = new AssignRoleViewModel
            {
                Students = students,
                Instructors = instructors,
                Users = _Users
            };

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if(roles.Count == 0)
                {
                    continue;
                }
                if (!roles.Contains("Admin"))
                    if (!roles.Contains("Admin"))
                    {
                        model.Users.Add(user);
                    }
                
            }

            return View(model);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageUser");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageUser");
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }

            return RedirectToAction("ManageUser");
        }

        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("AssignRole"); 
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count != 0)
            {
                TempData["ErrorMessage"] = "User already have a role.";

            }
            else
            {
                var result = await _userManager.AddToRoleAsync(user, model.Role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"User added to {model.Role} role.";
                    return RedirectToAction("AssignRole");
                }

                TempData["ErrorMessage"] = "Failed to add user to role.";
            }
            return RedirectToAction("AssignRole"); 
        }

        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<IActionResult> ConfirmAccount(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Email confirmed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error confirming email.";
            }

            return RedirectToAction(nameof(Manageuser));
        }


        [HttpPost]
        public async Task<IActionResult> UnconfirmAccount(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.EmailConfirmed = false;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Email Incative successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error Incative email.";
            }

            return RedirectToAction(nameof(Manageuser));
        }








        // Remove Role Action
        [HttpPost]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                TempData["ErrorMessage"] = "Invalid user or role.";
                return RedirectToAction("AssignRole");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("AssignRole");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to remove role.";
            }
            else
            {
                TempData["SuccessMessage"] = "Role removed successfully.";
            }

            return RedirectToAction("AssignRole"); // Redirect to the view showing the updated user roles
        }

        // Update Role Action
        [HttpPost]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
            {
                TempData["ErrorMessage"] = "Invalid user or role.";
                return RedirectToAction("AssignRole");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("AssignRole");
            }

            // Assuming you want to update the role of a user to a new one
            // First, remove all roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add the new role
            var result = await _userManager.AddToRoleAsync(user, newRole);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to update role.";
            }
            else
            {
                TempData["SuccessMessage"] = "Role updated successfully.";
            }

            return RedirectToAction("AssignRole"); // Redirect to the view showing the updated user roles
        }




    }
}