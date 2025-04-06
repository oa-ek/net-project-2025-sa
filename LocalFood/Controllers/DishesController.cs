using LocalFood.Data;
using LocalFood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalFood.Controllers
{
    [Authorize]
    public class DishesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DishesController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DishesController(ApplicationDbContext context, ILogger<DishesController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // Головна сторінка — список страв (фільтрація по restaurantId, якщо задано)
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? restaurantId)
        {
            var dishesQuery = _context.Dishes
                .Include(d => d.RestaurantDishes)
                .ThenInclude(rd => rd.Restaurant)
                .AsQueryable();

            if (restaurantId.HasValue && restaurantId.Value > 0)
            {
                dishesQuery = dishesQuery.Where(d =>
                    d.RestaurantDishes.Any(rd => rd.RestaurantId == restaurantId));
                ViewBag.RestaurantId = restaurantId;
            }

            return View(await dishesQuery.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create(int restaurantId)
        {
            ViewBag.RestaurantId = restaurantId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Dish dish, int restaurantId, IFormFile? image)
        {
            if (!ModelState.IsValid)
                return View(dish);

            if (image != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(image.FileName)
                             + "_" + Guid.NewGuid().ToString().Substring(0, 8)
                             + Path.GetExtension(image.FileName);

                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "dishes");
                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                dish.ImageUrl = "/images/dishes/" + fileName;
            }

            _context.Dishes.Add(dish);
            await _context.SaveChangesAsync();

            _context.RestaurantDishes.Add(new RestaurantDish
            {
                DishId = dish.DishId,
                RestaurantId = restaurantId
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Menu", "Restaurants", new { id = restaurantId });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var dish = await _context.Dishes
                .Include(d => d.RestaurantDishes)
                .FirstOrDefaultAsync(d => d.DishId == id);

            if (dish == null) return NotFound();

            ViewBag.RestaurantId = dish.RestaurantDishes.FirstOrDefault()?.RestaurantId ?? 0;
            return View(dish);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Dish dish, int restaurantId, IFormFile? image)
        {
            if (id != dish.DishId) return NotFound();

            var existing = await _context.Dishes.FindAsync(id);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
                return View(dish);

            existing.Name = dish.Name;
            existing.Description = dish.Description;
            existing.Price = dish.Price;

            if (image != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(image.FileName)
                             + "_" + Guid.NewGuid().ToString().Substring(0, 8)
                             + Path.GetExtension(image.FileName);

                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "dishes");
                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                existing.ImageUrl = "/images/dishes/" + fileName;
            }

            _context.Update(existing);

            var rel = await _context.RestaurantDishes.FirstOrDefaultAsync(r => r.DishId == id);
            if (rel != null)
            {
                rel.RestaurantId = restaurantId;
            }
            else
            {
                _context.RestaurantDishes.Add(new RestaurantDish
                {
                    DishId = id,
                    RestaurantId = restaurantId
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Menu", "Restaurants", new { id = restaurantId });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var dish = await _context.Dishes
                .Include(d => d.RestaurantDishes)
                .ThenInclude(rd => rd.Restaurant)
                .FirstOrDefaultAsync(d => d.DishId == id);

            if (dish == null) return NotFound();

            return View(dish);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish != null)
            {
                // 🧹 Видалення фото
                if (!string.IsNullOrEmpty(dish.ImageUrl))
                {
                    var fullImagePath = Path.Combine(_webHostEnvironment.WebRootPath, dish.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(fullImagePath))
                    {
                        System.IO.File.Delete(fullImagePath);
                    }
                }

                // Видалення зв'язку з ресторанами
                var relations = _context.RestaurantDishes.Where(rd => rd.DishId == id);
                _context.RestaurantDishes.RemoveRange(relations);

                _context.Dishes.Remove(dish);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
