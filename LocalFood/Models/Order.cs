using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalFood.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Ім'я клієнта є обов'язковим.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Телефон є обов'язковим.")]
        [Phone(ErrorMessage = "Введіть правильний номер телефону.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Адреса є обов'язковою.")]
        public string Address { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0; 

        [Required(ErrorMessage = "Оберіть статус замовлення.")]
        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public virtual OrderStatus? OrderStatus { get; set; }

        [Required(ErrorMessage = "Оберіть ресторан.")]
        public int RestaurantId { get; set; }

        [ForeignKey("RestaurantId")]
        public virtual Restaurant? Restaurant { get; set; }

        public virtual ICollection<OrderItem>? OrderItems { get; set; } = new List<OrderItem>();
    }
}
