using LocalFood.Data;
using LocalFood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LocalFood.Controllers
{
    [Authorize(Roles = "Courier")]
    public class CourierController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourierController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🏠 Головна сторінка кур’єра
        public IActionResult Index()
        {
            return View();
        }

        // 📦 Замовлення до доставки
        public async Task<IActionResult> OrdersToDeliver()
        {
            var currentCourierId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderStatus)
                .Where(o =>
                    o.StatusId == 1 || o.StatusId == 2 || // Прийнято або Готується
                    (o.StatusId == 3 && o.CourierId == currentCourierId)) // Якщо вже передано кур’єру, то лише якщо це його
                .ToListAsync();

            ViewBag.CurrentCourierId = currentCourierId;

            return View(orders);
        }

        // ▶️ Почати доставку
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartDelivery(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            if (order.StatusId != 1 && order.StatusId != 2)
                return BadRequest("Це замовлення не готове до доставки");

            order.StatusId = 3; // Передано кур'єру
            order.CourierId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(OrdersToDeliver));
        }

        // ✅ Завершити доставку
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteDelivery(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            var currentCourierId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (order.CourierId != currentCourierId)
                return Forbid();

            order.StatusId = 5; // Доставлено

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(OrdersToDeliver));
        }
    }
}
