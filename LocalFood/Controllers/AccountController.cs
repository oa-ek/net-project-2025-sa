using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LocalFood.ViewModels;
using System.Security.Claims;
using LocalFood.Data;
using Microsoft.EntityFrameworkCore;
using LocalFood.Models;

namespace LocalFood.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // === LOGIN ===
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Користувача з такою поштою не знайдено");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return await RedirectToRoleBasedHome(user);

            ModelState.AddModelError("", "Невірний логін або пароль");
            return View(model);
        }

        // === REGISTER ===
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Ця електронна пошта вже використовується.");
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (createResult.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole("User"));

                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return await RedirectToRoleBasedHome(user);
            }

            foreach (var error in createResult.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // === LOGOUT ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // === ACCESS DENIED ===
        public IActionResult AccessDenied() => View();

        // === DEV PASSWORD RESET ===
        [HttpGet]
        public IActionResult DevResetPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DevResetPassword(string email, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Користувача не знайдено");
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                ViewBag.Message = "✅ Пароль успішно скинуто!";
            }
            else
            {
                ViewBag.Message = "❌ Помилка при скиданні:";
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
            }

            return View();
        }

        // === GOOGLE LOGIN ===
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Помилка провайдера: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                return await RedirectToRoleBasedHome(user);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
                var existingUser = await _userManager.FindByEmailAsync(email);

                if (existingUser != null)
                {
                    await _userManager.AddLoginAsync(existingUser, info);
                    await _signInManager.SignInAsync(existingUser, isPersistent: false);
                    return await RedirectToRoleBasedHome(existingUser);
                }
                else
                {
                    var newUser = new IdentityUser { UserName = email, Email = email };
                    var createResult = await _userManager.CreateAsync(newUser);

                    if (createResult.Succeeded)
                    {
                        if (!await _roleManager.RoleExistsAsync("User"))
                            await _roleManager.CreateAsync(new IdentityRole("User"));

                        await _userManager.AddToRoleAsync(newUser, "User");
                        await _signInManager.SignInAsync(newUser, isPersistent: false);
                        return await RedirectToRoleBasedHome(newUser);
                    }
                }
            }

            return RedirectToAction(nameof(Login));
        }

        // === Редірект на головну відповідно до ролі ===
        private async Task<IActionResult> RedirectToRoleBasedHome(IdentityUser user)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("UsersList", "Admin");

            if (await _userManager.IsInRoleAsync(user, "Courier"))
                return RedirectToAction("Index", "Courier");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // Профіль користувача
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            // Замовлення (останні 5)
            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    OrderStatus = new OrderStatusViewModel { Name = o.OrderStatus.Name },
                    Restaurant = new RestaurantViewModel { Name = o.Restaurant.Name },
                    TotalAmount = o.TotalAmount
                }).ToListAsync();

            int ordersCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);
            decimal totalSpent = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var registrationDate = userProfile?.User?.LockoutEnd?.DateTime ?? DateTime.Now; // за потреби заміни

            // Збережені адреси
            var savedAddresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .Select(a => new AddressViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    FullAddress = a.FullAddress,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude
                }).ToListAsync();

            // Парсимо ім'я та прізвище
            var fullName = userProfile?.FullName ?? "";
            var firstName = "";
            var lastName = "";
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var nameParts = fullName.Split(' ');
                firstName = nameParts.Length > 0 ? nameParts[0] : "";
                lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";
            }

            var model = new ManageProfileViewModel
            {
                Email = user.Email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = userProfile?.Phone ?? user.PhoneNumber ?? "",
                OrdersCount = ordersCount,
                TotalSpent = totalSpent,
                RegistrationDate = registrationDate,
                RecentOrders = orders,
                SavedAddresses = savedAddresses
            };

            if (TempData.ContainsKey("PasswordUpdateSuccess"))
                ViewBag.PasswordUpdateSuccess = TempData["PasswordUpdateSuccess"];

            if (TempData.ContainsKey("ProfileUpdateSuccess"))
                ViewBag.ProfileUpdateSuccess = TempData["ProfileUpdateSuccess"];
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ManageProfileViewModel model)
        {
            // Діагностика валідності моделі
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage));
                System.Diagnostics.Debug.WriteLine("ModelState INVALID: " + errors);
                return View("Manage", model);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ModelState VALID");
            }

            System.Diagnostics.Debug.WriteLine("=== UpdateProfile CALLED ===");
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            System.Diagnostics.Debug.WriteLine("User.Id: " + user.Id);

            // --- ГОЛОВНЕ: створити UserProfile, якщо його нема ---
            var existingProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);
            if (existingProfile == null)
            {
                existingProfile = new UserProfile { UserId = user.Id };
                _context.UserProfiles.Add(existingProfile);
            }

            existingProfile.FullName = $"{model.FirstName} {model.LastName}".Trim();
            existingProfile.Phone = model.PhoneNumber?.Trim();

            // Оновлюємо телефон у AspNetUsers
            if (user.PhoneNumber != model.PhoneNumber)
            {
                user.PhoneNumber = model.PhoneNumber?.Trim();
                await _userManager.UpdateAsync(user);
            }

            System.Diagnostics.Debug.WriteLine("Додаю профіль: " + existingProfile.UserId + " " + existingProfile.FullName + " " + existingProfile.Phone);

            try
            {
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Збереження УСПІШНЕ! UserId: " + existingProfile.UserId);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ПОМИЛКА при збереженні: " + ex.Message);
                throw; // (або можеш повернути View з повідомленням)
            }

            TempData["ProfileUpdateSuccess"] = "Дані профілю оновлено!";
            return RedirectToAction("Manage");
        }
     


         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (NewPassword != ConfirmPassword)
            {
                TempData["PasswordUpdateSuccess"] = "Паролі не співпадають!";
                return RedirectToAction("Manage");
            }

            var result = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);

            if (result.Succeeded)
                TempData["PasswordUpdateSuccess"] = "Пароль змінено успішно!";
            else
                TempData["PasswordUpdateSuccess"] = "Сталась помилка: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Manage");
        }

                    // Додати адресу
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(string name, string fullAddress, double? latitude, double? longitude)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var address = new Address
            {
                UserId = user.Id,
                Name = name,
                FullAddress = fullAddress,
                Latitude = latitude,
                Longitude = longitude
            };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage");
        }

        // Оновити адресу
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAddress(int addressId, string name, string fullAddress, double? latitude, double? longitude)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var address = _context.Addresses.FirstOrDefault(a => a.Id == addressId && a.UserId == user.Id);
            if (address != null)
            {
                address.Name = name;
                address.FullAddress = fullAddress;
                address.Latitude = latitude;
                address.Longitude = longitude;
                _context.Update(address);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Manage");
        }

        // Видалити адресу
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var address = _context.Addresses.FirstOrDefault(a => a.Id == addressId && a.UserId == user.Id);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Manage");
        }

                     
    }
}
