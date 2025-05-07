using LocalFood.API.Data;
using LocalFood.API.Models;
using LocalFood.API.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalFood.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public OrdersController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetAll()
        {
            var list = await _db.Orders
                .Include(o => o.OrderItems)
                .Select(o => new OrderDto {
                    OrderId = o.OrderId,
                    CustomerName = o.CustomerName,
                    Phone = o.Phone,
                    Address = o.Address,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    StatusId = o.StatusId,
                    RestaurantId = o.RestaurantId,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        DishId = oi.DishId,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> Get(int id)
        {
            var o = await _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (o == null) return NotFound();
            return Ok(new OrderDto {
                OrderId = o.OrderId,
                CustomerName = o.CustomerName,
                Phone = o.Phone,
                Address = o.Address,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                StatusId = o.StatusId,
                RestaurantId = o.RestaurantId,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    DishId = oi.DishId,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create(OrderDto dto)
        {
            try
            {
                if (dto.RestaurantId == 0)
                    return BadRequest("Ресторан не вибрано");
                if (dto.StatusId == 0)
                    return BadRequest("Статус не вибрано");

                var o = new Order
                {
                    CustomerName = dto.CustomerName,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    OrderDate = DateTime.Now,
                    TotalAmount = 0,
                    StatusId = dto.StatusId,
                    RestaurantId = dto.RestaurantId
                };

                _db.Orders.Add(o);
                await _db.SaveChangesAsync();

                dto.OrderId = o.OrderId;
                return CreatedAtAction(nameof(Get), new { id = o.OrderId }, dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR creating order: {ex.Message}");
                return StatusCode(500, "Internal error: " + ex.Message);
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, OrderDto dto)
        {
            if (id != dto.OrderId) return BadRequest();
            var o = await _db.Orders.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.OrderId == id);
            if (o == null) return NotFound();

            o.CustomerName = dto.CustomerName;
            o.Phone = dto.Phone;
            o.Address = dto.Address;
            o.StatusId = dto.StatusId;
            o.RestaurantId = dto.RestaurantId;
            o.TotalAmount = o.OrderItems.Sum(oi => oi.Price * oi.Quantity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var o = await _db.Orders.FindAsync(id);
            if (o == null) return NotFound();
            _db.OrderItems.RemoveRange(_db.OrderItems.Where(oi => oi.OrderId == id));
            _db.Orders.Remove(o);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
