using LMMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
        [HttpGet]
        public IActionResult AssignRole()
        {
            var model = new AssignRoleViewModel
            {
                Users = _userManager.Users.ToList()
            };
            return View(model);
        }

        // Handle form submission
        [HttpPost]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                model.Users = await _userManager.Users.ToListAsync(); // Repopulate the users list in case of validation errors
                return View(model);
            }

            // Try to find the user by ID
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound($"User with ID {model.UserId} not found.");
            }

            // Add the user to the role
            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (result.Succeeded)
            {
                return Ok($"User added to {model.Role} role successfully.");
            }

            // Return a bad request if the operation failed
            return BadRequest($"Failed to add user to {model.Role} role.");
        }

    }
}