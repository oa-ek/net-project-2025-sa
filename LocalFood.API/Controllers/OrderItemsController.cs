using LocalFood.API.Data;
using LocalFood.API.Models;
using LocalFood.API.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalFood.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public OrderItemsController(ApplicationDbContext db) => _db = db;

        [HttpPost]
        public async Task<ActionResult<OrderItemDto>> Create(OrderItemDto dto)
        {
            var dish = await _db.Dishes.FindAsync(dto.DishId);
            if (dish == null) return NotFound("Dish not found");

            var oi = new OrderItem {
                OrderId = dto.OrderId,
                DishId = dto.DishId,
                Quantity = dto.Quantity,
                Price = dish.Price
            };
            _db.OrderItems.Add(oi);
            await _db.SaveChangesAsync();

            // оновлюємо суму замовлення:
            var order = await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderId == dto.OrderId);
            if (order != null) {
                order.TotalAmount = order.OrderItems.Sum(x => x.Price * x.Quantity);
                await _db.SaveChangesAsync();
            }

            dto.Id = oi.Id;
            dto.Price = oi.Price;
            return CreatedAtAction(null, dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var oi = await _db.OrderItems.FindAsync(id);
            if (oi == null) return NotFound();

            int orderId = oi.OrderId;
            _db.OrderItems.Remove(oi);
            await _db.SaveChangesAsync();

            var order = await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order != null) {
                order.TotalAmount = order.OrderItems.Sum(x => x.Price * x.Quantity);
                await _db.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
