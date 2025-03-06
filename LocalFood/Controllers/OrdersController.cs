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
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Відкрито сторінку списку замовлень");

            var orders = _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderStatus);

            return View(await orders.ToListAsync());
        }

        public IActionResult Create()
        {
            _logger.LogInformation("Відкрито сторінку створення замовлення");

            ViewBag.RestaurantId = new SelectList(_context.Restaurants, "RestaurantId", "Name");
            ViewBag.StatusList = new SelectList(_context.OrderStatuses, "StatusId", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerName,Phone,Address,RestaurantId,StatusId")] Order order)
        {
            _logger.LogInformation("Отримано запит на створення замовлення");

            if (order.RestaurantId == 0)
            {
                ModelState.AddModelError("RestaurantId", "Оберіть ресторан.");
                _logger.LogWarning("Помилка валідації: не вибрано ресторан");
            }
            if (order.StatusId == 0)
            {
                ModelState.AddModelError("StatusId", "Оберіть статус замовлення.");
                _logger.LogWarning("Помилка валідації: не вибрано статус замовлення");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState містить помилки: {Errors}",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                ViewBag.RestaurantId = new SelectList(_context.Restaurants, "RestaurantId", "Name", order.RestaurantId);
                ViewBag.StatusList = new SelectList(_context.OrderStatuses, "StatusId", "Name", order.StatusId);
                return View(order);
            }

            order.OrderDate = DateTime.Now;
            order.TotalAmount = 0; // Початково 0, бо страви ще не додані
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Замовлення успішно створене з ID: {OrderId}", order.OrderId);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Dish)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            ViewBag.RestaurantId = new SelectList(_context.Restaurants, "RestaurantId", "Name", order.RestaurantId);
            ViewBag.StatusList = new SelectList(_context.OrderStatuses, "StatusId", "Name", order.StatusId);

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.OrderId) return NotFound();

            var existingOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (existingOrder == null) return NotFound();

            //  Обчислюємо загальну суму перед збереженням
            existingOrder.TotalAmount = existingOrder.OrderItems.Sum(oi => oi.Price * oi.Quantity);
            existingOrder.CustomerName = order.CustomerName;
            existingOrder.Phone = order.Phone;
            existingOrder.Address = order.Address;
            existingOrder.StatusId = order.StatusId;
            existingOrder.RestaurantId = order.RestaurantId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Track(int id)
        {
            _logger.LogInformation("Відкрито сторінку відстеження замовлення з ID: {OrderId}", id);

            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Dish)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                _logger.LogWarning("Замовлення з ID {OrderId} не знайдено", id);
                return NotFound();
            }

            return View(order);
        }

    }
}
