// Controllers/OrdersController.cs
using LocalFood.Data;
using LocalFood.Extensions;
using LocalFood.Models;
using LocalFood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LocalFood.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<OrdersController> _log;

        public OrdersController(ApplicationDbContext db, ILogger<OrdersController> log)
        {
            _db  = db;
            _log = log;
        }

        // Manager бачить тільки замовлення свого ресторану
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Index()
        {
            var mgr = User.GetUserId();
            var orders = await _db.Orders
                .Include(o => o.Restaurant).Include(o => o.OrderStatus)
                .Where(o => o.Restaurant.ManagerId == mgr)
                .ToListAsync();
            return View(orders);
        }

        // User — тільки свої
        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyOrders()
        {
            var uid = User.GetUserId();
            var orders = await _db.Orders
                .Where(o => o.UserId == uid)
                .Include(o => o.Restaurant).Include(o => o.OrderStatus)
                .ToListAsync();
            return View(orders);
        }

        // Create — всі авторизовані
        [Authorize]
        public IActionResult Create()
        {
            ViewBag.RestaurantId = new SelectList(_db.Restaurants,    "RestaurantId","Name");
            ViewBag.StatusList   = new SelectList(_db.OrderStatuses,  "StatusId",    "Name");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public async Task<IActionResult> Create([Bind("CustomerName,Phone,Address,RestaurantId,StatusId,Latitude,Longitude")] Order o)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RestaurantId = new SelectList(_db.Restaurants,   "RestaurantId","Name", o.RestaurantId);
                ViewBag.StatusList   = new SelectList(_db.OrderStatuses, "StatusId",   "Name", o.StatusId);
                return View(o);
            }

            o.UserId      = User.GetUserId();
            o.OrderDate   = DateTime.Now;
            o.TotalAmount = 0;

            _db.Orders.Add(o);
            await _db.SaveChangesAsync();
            return RedirectToAction("Track", new { id = o.OrderId });
        }

        // Edit (Manager)
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var mgr = User.GetUserId();
            var o = await _db.Orders
                .Include(o => o.Restaurant).Include(o => o.OrderItems)
                .FirstOrDefaultAsync(x => x.OrderId == id && x.Restaurant.ManagerId == mgr);
            if (o == null) return Forbid();

            ViewBag.RestaurantId = new SelectList(_db.Restaurants,   "RestaurantId","Name", o.RestaurantId);
            ViewBag.StatusList   = new SelectList(_db.OrderStatuses, "StatusId",  "Name",   o.StatusId);
            return View(o);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id, Order m)
        {
            var mgr = User.GetUserId();
            var existing = await _db.Orders
               .Include(o => o.Restaurant).Include(o => o.OrderItems)
               .FirstOrDefaultAsync(o => o.OrderId == id && o.Restaurant.ManagerId == mgr);
            if (existing == null) return Forbid();

            existing.CustomerName = m.CustomerName;
            existing.Phone        = m.Phone;
            existing.Address      = m.Address;
            existing.StatusId     = m.StatusId;
            existing.RestaurantId = m.RestaurantId;
            existing.Latitude     = m.Latitude;
            existing.Longitude    = m.Longitude;
            existing.TotalAmount  = existing.OrderItems.Sum(i => i.Price * i.Quantity);

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Track — User, Manager (свій), Courier
        [Authorize]
        public async Task<IActionResult> Track(int id)
        {
            _log.LogInformation("Track order {Id}", id);
            var o = await _db.Orders
                .Include(x => x.Restaurant)
                .Include(x => x.OrderStatus)
                .Include(x => x.OrderItems).ThenInclude(oi => oi.Dish)
                .FirstOrDefaultAsync(x => x.OrderId == id);
            if (o == null) return NotFound();

            var uid = User.GetUserId();
            var isMgr     = User.IsInRole("Manager") && o.Restaurant.ManagerId == uid;
            var isUser    = User.IsInRole("User")    && o.UserId     == uid;
            var isCourier = User.IsInRole("Courier");
            if (!(isMgr || isUser || isCourier))
                return Forbid();

            if (User.IsInRole("Manager") || User.IsInRole("Courier"))
            {
                ViewBag.StatusList = await _db.OrderStatuses
                    .Select(s => new SelectListItem { Value = s.StatusId.ToString(), Text = s.Name })
                    .ToListAsync();
            }

            return View(o);
        }

        // UpdateStatus
        [Authorize(Roles = "Manager,Courier")]
        public async Task<IActionResult> UpdateStatus(int orderId, int statusId)
        {
            var o = await _db.Orders
                .Include(x => x.Restaurant)
                .FirstOrDefaultAsync(x => x.OrderId == orderId);
            if (o == null) return NotFound();

            var uid = User.GetUserId();
            if (User.IsInRole("Manager") && o.Restaurant.ManagerId != uid)
                return Forbid();

            o.StatusId = statusId;
            if (statusId == 3 && User.IsInRole("Courier"))
                o.CourierId = uid;

            await _db.SaveChangesAsync();
            return RedirectToAction("Track", new { id = orderId });
        }

        // Courier
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> ToDeliver()
        {
            var list = await _db.Orders
                .Include(o => o.Restaurant).Include(o => o.OrderStatus)
                .Where(o => o.StatusId < 3)
                .ToListAsync();
            return View(list);
        }
    }
}
