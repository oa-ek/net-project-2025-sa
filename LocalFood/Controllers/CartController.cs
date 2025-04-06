﻿using LocalFood.Data;
using LocalFood.Models;
using LocalFood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LocalFood.Controllers
{
    [Authorize(Roles = "User")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.CartItems
                .Include(c => c.Dish)
                .Where(c => c.UserId == userId)
                .ToListAsync();
            return View(cartItems);
        }

        public async Task<IActionResult> AddToCart(int dishId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.DishId == dishId && c.UserId == userId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    DishId = dishId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null && cartItem.UserId == userId)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userProfile = await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var vm = new CheckoutViewModel
            {
                FullName = userProfile?.FullName ?? "",
                Phone = userProfile?.User?.PhoneNumber ?? "",
                Address = userProfile?.Address ?? ""
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid");
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.CartItems
                .Include(c => c.Dish)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                ModelState.AddModelError("", "Ваш кошик порожній.");
                return View(model);
            }

            // Оновлення адреси в профілі
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (userProfile != null)
            {
                userProfile.Address = model.Address;
                _context.UserProfiles.Update(userProfile);
            }

            // Визначення ресторану (по першій страві)
            var firstDishId = cartItems.First().DishId;
            var restaurantDish = await _context.RestaurantDishes.FirstOrDefaultAsync(rd => rd.DishId == firstDishId);
            int restaurantId = restaurantDish?.RestaurantId ?? 1;

            // Створення замовлення
            var order = new Order
            {
                UserId = userId,
                CustomerName = model.FullName,
                Phone = model.Phone,
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                RestaurantId = restaurantId,
                StatusId = 1, // "Прийнято"
                OrderDate = DateTime.Now,
                TotalAmount = 0
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // щоб отримати OrderId

            decimal total = 0;
            var orderItemsList = new List<OrderItem>();
            foreach (var ci in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    DishId = ci.DishId,
                    Quantity = ci.Quantity,
                    Price = ci.Dish.Price
                };
                total += orderItem.Price * orderItem.Quantity;
                orderItemsList.Add(orderItem);
            }

            _context.OrderItems.AddRange(orderItemsList);

            // Оновлюємо суму
            order.TotalAmount = total;
            _context.Orders.Update(order);

            // Чистимо кошик
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Замовлення #{OrderId} створено користувачем {UserId}", order.OrderId, userId);

            return RedirectToAction("Track", "Orders", new { id = order.OrderId });
        }
    }
}
