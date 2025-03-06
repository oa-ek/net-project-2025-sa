using LocalFood.Data;
using LocalFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace LocalFood.Controllers
{
    public class DishesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DishesController> _logger;

        public DishesController(ApplicationDbContext context, ILogger<DishesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Відображення списку страв з ресторанами
        public async Task<IActionResult> Index()
        {
            var dishes = _context.Dishes.Include(d => d.RestaurantDishes)
                              .ThenInclude(rd => rd.Restaurant);
            return View(await dishes.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.RestaurantId = new SelectList(_context.Restaurants, "RestaurantId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dish dish, int RestaurantId)
        {
            if (RestaurantId == 0)
            {
                ModelState.AddModelError("RestaurantId", "Оберіть ресторан зі списку.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(dish);
                await _context.SaveChangesAsync();

                // Створення зв’язку через RestaurantDish
                var rd = new RestaurantDish { DishId = dish.DishId, RestaurantId = RestaurantId };
                _context.RestaurantDishes.Add(rd);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.RestaurantId = new SelectList(_context.Restaurants, "RestaurantId", "Name", RestaurantId);
            return View(dish);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dish = await _context.Dishes.Include(d => d.RestaurantDishes).FirstOrDefaultAsync(d => d.DishId == id);
            if (dish == null) return NotFound();

            var restaurantId = dish.RestaurantDishes.FirstOrDefault()?.RestaurantId ?? 0;
            ViewBag.RestaurantId = new SelectList(_context.Restaurants, "RestaurantId", "Name", restaurantId);
            return View(dish);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Dish dish, int RestaurantId)
        {
            if (id != dish.DishId) return NotFound();

            if (RestaurantId == 0)
            {
                ModelState.AddModelError("RestaurantId", "Оберіть ресторан зі списку.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(dish);
                await _context.SaveChangesAsync();

                var rd = await _context.RestaurantDishes.FirstOrDefaultAsync(x => x.DishId == dish.DishId);
                if (rd != null)
                {
                    rd.RestaurantId = RestaurantId;
                    _context.Update(rd);
                }
                else
                {
                    rd = new RestaurantDish { DishId = dish.DishId, RestaurantId = RestaurantId };
                    _context.RestaurantDishes.Add(rd);
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.RestaurantId = new SelectList(_context.Restaurants, "RestaurantId", "Name", RestaurantId);
            return View(dish);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var dish = await _context.Dishes.Include(d => d.RestaurantDishes)
                              .ThenInclude(rd => rd.Restaurant)
                              .FirstOrDefaultAsync(d => d.DishId == id);
            if (dish == null) return NotFound();
            return View(dish);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish != null)
            {
                var rdList = _context.RestaurantDishes.Where(rd => rd.DishId == id);
                _context.RestaurantDishes.RemoveRange(rdList);

                _context.Dishes.Remove(dish);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
