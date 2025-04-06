using LocalFood.Data;
using LocalFood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LocalFood.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderStatus)
                .ToListAsync();
            return View(orders);
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.GetUserId();
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderStatus)
                .ToListAsync();
            return View(orders);
        }

        [Authorize]
        public IActionResult Create()
        {
            ViewBag.RestaurantId = new SelectList(_context.Restaurants, "RestaurantId", "Name");
            ViewBag.StatusList = new SelectList(_context.OrderStatuses, "StatusId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("CustomerName,Phone,Address,RestaurantId,StatusId,Latitude,Longitude")] Order order)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RestaurantId = new SelectList(_context.Restaurants, "RestaurantId", "Name", order.RestaurantId);
                ViewBag.StatusList = new SelectList(_context.OrderStatuses, "StatusId", "Name", order.StatusId);
                return View(order);
            }

            order.UserId = User.GetUserId();
            order.OrderDate = DateTime.Now;
            order.TotalAmount = 0;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Track", new { id = order.OrderId });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound();

            ViewBag.RestaurantId = new SelectList(_context.Restaurants, "RestaurantId", "Name", order.RestaurantId);
            ViewBag.StatusList = new SelectList(_context.OrderStatuses, "StatusId", "Name", order.StatusId);

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.OrderId) return NotFound();

            var existing = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (existing == null) return NotFound();

            existing.CustomerName = order.CustomerName;
            existing.Phone = order.Phone;
            existing.Address = order.Address;
            existing.StatusId = order.StatusId;
            existing.RestaurantId = order.RestaurantId;
            existing.Latitude = order.Latitude;
            existing.Longitude = order.Longitude;
            existing.TotalAmount = existing.OrderItems.Sum(oi => oi.Quantity * oi.Price);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
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

            var currentUserId = User.GetUserId();

            if (!User.IsInRole("Admin") && !User.IsInRole("Courier") && order.UserId != currentUserId)
            {
                return Forbid(); // або NotFound();
            }

            // Передати список статусів, тільки якщо це адмін або кур'єр
            if (User.IsInRole("Admin") || User.IsInRole("Courier"))
            {
                ViewBag.StatusList = await _context.OrderStatuses
                    .Select(s => new SelectListItem
                    {
                        Value = s.StatusId.ToString(),
                        Text = s.Name
                    }).ToListAsync();
            }

            return View(order);
        }


        [Authorize(Roles = "Admin,Courier")]
        public async Task<IActionResult> UpdateStatus(int orderId, int statusId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.StatusId = statusId;

            if (statusId == 3 && User.IsInRole("Courier")) // Передано кур’єру
            {
                order.CourierId = User.GetUserId();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Track", new { id = orderId });
        }
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> ToDeliver()
        {
            var activeOrders = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderStatus)
                .Where(o => o.StatusId != 3 && o.StatusId != 4 && o.StatusId != 5)
                .ToListAsync();

            return View(activeOrders);
}

    }

    public static class UserExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }


}
