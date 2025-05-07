using LocalFood.API.Data;
using LocalFood.API.Models;
using LocalFood.API.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalFood.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public RestaurantsController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<RestaurantDto>>> GetAll()
        {
            var list = await _db.Restaurants
                .Select(r => new RestaurantDto {
                    RestaurantId = r.RestaurantId,
                    Name = r.Name,
                    Description = r.Description,
                    Address = r.Address
                })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantDto>> Get(int id)
        {
            var r = await _db.Restaurants.FindAsync(id);
            if (r == null) return NotFound();
            return Ok(new RestaurantDto {
                RestaurantId = r.RestaurantId,
                Name = r.Name,
                Description = r.Description,
                Address = r.Address
            });
        }

        [HttpPost]
        public async Task<ActionResult<RestaurantDto>> Create(RestaurantDto dto)
        {
            var entity = new Restaurant {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address
            };
            _db.Restaurants.Add(entity);
            await _db.SaveChangesAsync();
            dto.RestaurantId = entity.RestaurantId;
            return CreatedAtAction(nameof(Get), new { id = entity.RestaurantId }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RestaurantDto dto)
        {
            if (id != dto.RestaurantId) return BadRequest();
            var entity = await _db.Restaurants.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Address = dto.Address;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Restaurants.FindAsync(id);
            if (entity == null) return NotFound();
            _db.Restaurants.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
