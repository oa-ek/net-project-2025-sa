using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public virtual ICollection<RestaurantDish> RestaurantDishes { get; set; } = new List<RestaurantDish>();
    }
}
