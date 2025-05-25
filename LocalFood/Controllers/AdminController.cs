using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using LocalFood.ViewModels;

namespace LocalFood.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Список всіх користувачів
        public async Task<IActionResult> UsersList()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                model.Add(new UserViewModel {
                    Id = u.Id,
                    Email = u.Email,
                    Roles = roles,
                    IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow
                });
            }

            return View(model);
        }

        // GET: /Admin/EditRoles/{id}
        public async Task<IActionResult> EditRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allRoles = _roleManager.Roles.Select(r => new SelectListItem {
                Value = r.Name, Text = r.Name
            }).ToList();

            var userRoles = await _userManager.GetRolesAsync(user);

            var vm = new EditRolesViewModel {
                UserId = user.Id,
                Email = user.Email,
                AllRoles = allRoles,
                Roles = userRoles.FirstOrDefault()  // беремо першу (або null)
            };
            return View(vm);
        }

        // POST: /Admin/EditRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            // 1) видаляємо всі старі ролі
            var current = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, current);

            // 2) додаємо одну обрану роль, якщо вона не пуста
            if (!string.IsNullOrWhiteSpace(model.Roles))
            {
                if (!await _roleManager.RoleExistsAsync(model.Roles))
                    await _roleManager.CreateAsync(new IdentityRole(model.Roles));
                await _userManager.AddToRoleAsync(user, model.Roles);
            }

            // 3) оновлюємо security stamp — щоб переконатися, що нова роль застосована при наступному запиті
            await _userManager.UpdateSecurityStampAsync(user);

            return RedirectToAction(nameof(UsersList));
        }

        // POST: /Admin/ToggleLock/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;

            if (isLocked)
            {
                // розблокувати
                user.LockoutEnd = null;
            }
            else
            {
                // блокувати — одночасно прибираємо ролі
                user.LockoutEnd = DateTimeOffset.MaxValue;
                var roles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, roles);
            }

            await _userManager.UpdateAsync(user);
            await _userManager.UpdateSecurityStampAsync(user);

            return RedirectToAction(nameof(UsersList));
        }
    }
}
