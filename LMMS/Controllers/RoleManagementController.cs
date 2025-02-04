using LMMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var result = await _userManager.AddToRoleAsync(user, model.Role);
                if (result.Succeeded)
                {
                    return Ok($"User added to {model.Role} role");
                }

                return BadRequest("Failed to add user to role");
            }

            model.Users = _userManager.Users.ToList(); // Repopulate the users list in case of validation errors
            return View(model);
        }
    }
}