using LocalFood.API.Data;
using LocalFood.API.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalFood.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderStatusesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public OrderStatusesController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<OrderStatusDto>>> GetAll()
        {
            return Ok(await _db.OrderStatuses
                .Select(s => new OrderStatusDto { StatusId = s.StatusId, Name = s.Name })
                .ToListAsync());
        }
    }
}
