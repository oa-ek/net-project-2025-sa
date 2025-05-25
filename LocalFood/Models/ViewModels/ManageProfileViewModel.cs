using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding; // Додай це!

namespace LocalFood.ViewModels
{
    public class ManageProfileViewModel
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "Ім'я обов'язкове")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Прізвище обов'язкове")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Телефон обов'язковий")]
        [Phone(ErrorMessage = "Введіть коректний номер телефону")]
        public string PhoneNumber { get; set; }

        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime RegistrationDate { get; set; }


        [BindNever]
        public List<OrderViewModel> RecentOrders { get; set; } = new();    // Ініціалізуй тут

        [BindNever]
        public List<AddressViewModel> SavedAddresses { get; set; } = new(); // І тут!
    
    }

    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatusViewModel OrderStatus { get; set; }
        public RestaurantViewModel Restaurant { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class AddressViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullAddress { get; set; }
        public double? Latitude { get; set; }    // ДОДАЙ ОСЬ ЦЕ
        public double? Longitude { get; set; }   // ДОДАЙ ОСЬ ЦЕ
    }

    public class OrderStatusViewModel
    {
        public string Name { get; set; }
    }

    public class RestaurantViewModel
    {
        public string Name { get; set; }
    }
}
