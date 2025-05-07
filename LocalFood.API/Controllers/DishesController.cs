using LocalFood.API.Data;
using LocalFood.API.Models;
using LocalFood.API.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalFood.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public DishesController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<DishDto>>> GetAll()
        {
            var list = await _db.Dishes
                .Include(d => d.RestaurantDishes)
                .ThenInclude(rd => rd.Restaurant)
                .Select(d => new DishDto
                {
                    DishId = d.DishId,
                    Name = d.Name,
                    Description = d.Description,
                    Price = d.Price,
                    RestaurantId = d.RestaurantDishes.FirstOrDefault() != null 
                                    ? d.RestaurantDishes.First().RestaurantId 
                                    : 0
                })
                .ToListAsync();

            return Ok(list);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<DishDto>> Get(int id)
        {
            var d = await _db.Dishes
                .Include(d => d.RestaurantDishes)
                .FirstOrDefaultAsync(d => d.DishId == id);
            if (d == null) return NotFound();
            return Ok(new DishDto {
                DishId = d.DishId,
                Name = d.Name,
                Description = d.Description,
                Price = d.Price,
                RestaurantId = d.RestaurantDishes.FirstOrDefault()?.RestaurantId ?? 0
            });
        }

        [HttpPost]
        public async Task<ActionResult<DishDto>> Create(DishDto dto)
        {
            var dish = new Dish {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price
            };
            _db.Dishes.Add(dish);
            await _db.SaveChangesAsync();

            _db.RestaurantDishes.Add(new RestaurantDish {
                DishId = dish.DishId,
                RestaurantId = dto.RestaurantId
            });
            await _db.SaveChangesAsync();

            dto.DishId = dish.DishId;
            return CreatedAtAction(nameof(Get), new { id = dish.DishId }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DishDto dto)
        {
            if (id != dto.DishId) return BadRequest();
            var dish = await _db.Dishes.FindAsync(id);
            if (dish == null) return NotFound();

            dish.Name = dto.Name;
            dish.Description = dto.Description;
            dish.Price = dto.Price;
            await _db.SaveChangesAsync();

            var rd = await _db.RestaurantDishes.FirstOrDefaultAsync(x => x.DishId == id);
            if (rd != null)
                rd.RestaurantId = dto.RestaurantId;
            else
                _db.RestaurantDishes.Add(new RestaurantDish { DishId = id, RestaurantId = dto.RestaurantId });

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dish = await _db.Dishes.FindAsync(id);
            if (dish == null) return NotFound();

            _db.RestaurantDishes.RemoveRange(_db.RestaurantDishes.Where(x => x.DishId == id));
            _db.Dishes.Remove(dish);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
