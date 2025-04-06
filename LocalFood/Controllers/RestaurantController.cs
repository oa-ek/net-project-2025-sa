using LocalFood.Data;
using LocalFood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace LocalFood.Controllers
{
    [Authorize]
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RestaurantsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Restaurants.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Restaurant restaurant, IFormFile? UploadImage)
        {
            if (ModelState.IsValid)
            {
                if (UploadImage != null)
                {
                    restaurant.ImagePath = await ProcessUploadedFileAsync(UploadImage);
                }

                _context.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(restaurant);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();
            return View(restaurant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Restaurant restaurant, IFormFile? UploadImage)
        {
            if (id != restaurant.RestaurantId) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.Restaurants.FindAsync(id);
                if (existing == null) return NotFound();

                // Оновлення властивостей
                existing.Name = restaurant.Name;
                existing.Description = restaurant.Description;
                existing.Address = restaurant.Address;

                if (UploadImage != null)
                {
                    existing.ImagePath = await ProcessUploadedFileAsync(UploadImage);
                }

                _context.Update(existing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(restaurant);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();
            return View(restaurant);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant != null)
            {
                // 🧹 Видалення фото
                if (!string.IsNullOrEmpty(restaurant.ImagePath))
                {
                    var fullImagePath = Path.Combine(_webHostEnvironment.WebRootPath, restaurant.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullImagePath))
                    {
                        System.IO.File.Delete(fullImagePath);
                    }
                }

                _context.Restaurants.Remove(restaurant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Метод обробки завантаженого файлу: генерує унікальне ім'я, створює потрібну папку,
        /// зберігає файл та повертає URL зображення.
        /// </summary>
        private async Task<string?> ProcessUploadedFileAsync(IFormFile UploadImage)
        {
            if (UploadImage == null || UploadImage.Length == 0)
            {
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(UploadImage.FileName)
                           + "_" + Guid.NewGuid().ToString().Substring(0, 8)
                           + Path.GetExtension(UploadImage.FileName);

            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "restaurants");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await UploadImage.CopyToAsync(stream);
            }

            return "/images/restaurants/" + fileName;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Menu(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.RestaurantDishes)
                .ThenInclude(rd => rd.Dish)
                .FirstOrDefaultAsync(r => r.RestaurantId == id);

            if (restaurant == null)
                return NotFound();

            return View(restaurant);
        }

    }
}
