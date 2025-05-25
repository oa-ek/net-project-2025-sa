using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalFood.Models
{
    public class Restaurant
    {
        [Key]
        public int RestaurantId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string ManagerId { get; set; }

        [ForeignKey(nameof(ManagerId))]
        public virtual IdentityUser Manager { get; set; }

        public string? ImagePath { get; set; }

        public virtual ICollection<RestaurantDish> RestaurantDishes { get; set; } = new List<RestaurantDish>();
    }
}
