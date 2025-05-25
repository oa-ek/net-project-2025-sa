// Controllers/RestaurantsController.cs
using LocalFood.Data;
using LocalFood.Models;
using LocalFood.ViewModels;
using LocalFood.Extensions;              // для User.GetUserId()
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LocalFood.Controllers
{
    [Authorize]
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public RestaurantsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db  = db;
            _env = env;
        }

        // GET: /Restaurants/Index
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // 1) Неавторизовані, User або Admin — бачать всі ресторани
            if (!User.Identity.IsAuthenticated
                || User.IsInRole("User")
                || User.IsInRole("Admin"))
            {
                var all = await _db.Restaurants.ToListAsync();
                return View(all);
            }

            // 2) Manager — бачить тільки свій ресторан
            if (User.IsInRole("Manager"))
            {
                var mgrId = User.GetUserId();
                var mine = await _db.Restaurants
                    .Where(r => r.ManagerId == mgrId)
                    .ToListAsync();
                return View(mine);
            }

            // 3) Інші ролі (наприклад Courier) — заборонено
            return Forbid();
        }

        // GET: /Restaurants/Create
        [Authorize(Roles = "Manager")]
        public IActionResult Create()
        {
            return View(new RestaurantCreateViewModel());
        }

        // POST: /Restaurants/Create
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(RestaurantCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Створюємо доменну модель і підставляємо ManagerId
            var restaurant = new Restaurant
            {
                Name        = vm.Name,
                Address     = vm.Address,
                Description = vm.Description,
                ManagerId   = User.GetUserId()
            };

            if (vm.UploadImage != null)
            {
                restaurant.ImagePath = await SaveFile(vm.UploadImage);
            }

            _db.Restaurants.Add(restaurant);
            await _db.SaveChangesAsync();

            // Повертаємося в Index — там менеджер побачить лише свій ресторан
            return RedirectToAction(nameof(Index));
        }

        // GET: /Restaurants/Edit/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var rest = await _db.Restaurants.FindAsync(id);
            if (rest == null || rest.ManagerId != User.GetUserId())
                return Forbid();

            var vm = new RestaurantEditViewModel
            {
                RestaurantId = rest.RestaurantId,
                Name = rest.Name,
                Address = rest.Address,
                Description = rest.Description,
                ImagePath = rest.ImagePath
            };
            return View(vm);
        }


        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id, RestaurantEditViewModel vm)
        {
            if (id != vm.RestaurantId)
                return BadRequest();

            var rest = await _db.Restaurants.FindAsync(id);
            if (rest == null || rest.ManagerId != User.GetUserId())
                return Forbid();

            if (!ModelState.IsValid)
                return View(vm);

            rest.Name = vm.Name;
            rest.Address = vm.Address;
            rest.Description = vm.Description;

            // Якщо новий файл завантажено
            if (vm.UploadImage != null)
            {
                // видаляємо старий файл
                if (!string.IsNullOrEmpty(rest.ImagePath))
                {
                    var full = Path.Combine(_env.WebRootPath, rest.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(full))
                        System.IO.File.Delete(full);
                }
                rest.ImagePath = await SaveFile(vm.UploadImage);
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: /Restaurants/Delete/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var rest = await _db.Restaurants.FindAsync(id);
            if (rest == null || rest.ManagerId != User.GetUserId())
                return Forbid();

            return View(rest);
        }

        // POST: /Restaurants/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rest = await _db.Restaurants.FindAsync(id);
            if (rest == null || rest.ManagerId != User.GetUserId())
                return Forbid();

            // Видаляємо файл, якщо був
            if (!string.IsNullOrEmpty(rest.ImagePath))
            {
                var full = Path.Combine(_env.WebRootPath, rest.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(full))
                    System.IO.File.Delete(full);
            }

            _db.Restaurants.Remove(rest);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Restaurants/Menu/5
        [AllowAnonymous]
        public async Task<IActionResult> Menu(int id)
        {
            var rest = await _db.Restaurants
                .Include(r => r.RestaurantDishes)
                .ThenInclude(rd => rd.Dish)
                .FirstOrDefaultAsync(r => r.RestaurantId == id);
            if (rest == null)
                return NotFound();

            return View(rest);
        }

        // ********** допоміжний метод **********
        private async Task<string> SaveFile(IFormFile file)
        {
            var fn  = Path.GetFileNameWithoutExtension(file.FileName)
                      + "_" + Guid.NewGuid().ToString("n").Substring(0, 8)
                      + Path.GetExtension(file.FileName);
            var dir = Path.Combine(_env.WebRootPath, "images", "restaurants");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, fn);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/images/restaurants/{fn}";
        }
    }
}
