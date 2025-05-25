using LocalFood.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LocalFood.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<RestaurantDish> RestaurantDishes { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Address> Addresses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RestaurantDish>()
                .HasOne(rd => rd.Restaurant)
                .WithMany(r => r.RestaurantDishes)
                .HasForeignKey(rd => rd.RestaurantId);

            modelBuilder.Entity<RestaurantDish>()
                .HasOne(rd => rd.Dish)
                .WithMany(d => d.RestaurantDishes)
                .HasForeignKey(rd => rd.DishId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Dish)
                .WithMany()
                .HasForeignKey(oi => oi.DishId);

            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { StatusId = 1, Name = "Прийнято" },
                new OrderStatus { StatusId = 2, Name = "Готується" },
                new OrderStatus { StatusId = 3, Name = "Передано кур’єру" },
                new OrderStatus { StatusId = 4, Name = "Доставляється" },
                new OrderStatus { StatusId = 5, Name = "Доставлено" }
            );
        }
    }
}
