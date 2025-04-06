using LocalFood.Data;
using LocalFood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LocalFood.Controllers
{
    [Authorize] // Лише для авторизованих
    public class OrderItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderItemsController> _logger;

        public OrderItemsController(ApplicationDbContext context, ILogger<OrderItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Create(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) return NotFound();

            // Можемо додати перевірку, що це замовлення належить поточному користувачу 
            // або ж що поточний користувач - Admin, якщо потрібно.

            ViewBag.DishId = new SelectList(_context.Dishes, "DishId", "Name");
            return View(new OrderItem { OrderId = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,DishId,Quantity")] OrderItem orderItem)
        {
            var dish = await _context.Dishes.FindAsync(orderItem.DishId);
            if (dish == null) return NotFound();

            orderItem.Price = dish.Price;
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

            // Оновити суму
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId);

            if (order != null)
            {
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Price * oi.Quantity);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Track", "Orders", new { id = orderItem.OrderId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null) return NotFound();

            // Перевірка доступу (чи поточний користувач - власник/адмін).
            // ...

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            // Оновити суму
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId);

            if (order != null)
            {
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Price * oi.Quantity);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Track", "Orders", new { id = orderItem.OrderId });
        }
    }
}
