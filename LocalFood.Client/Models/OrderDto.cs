using System;
using System.Collections.Generic;

namespace LocalFood.Client.Models
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int StatusId { get; set; }
        public int RestaurantId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
