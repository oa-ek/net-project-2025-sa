using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalFood.API.Models
{
    public class Dish
    {
        [Key]
        public int DishId { get; set; }

        [Required(ErrorMessage = "Назва страви є обов'язковою.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Опис страви є обов'язковим.")]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Ціна повинна бути більшою за 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public virtual ICollection<RestaurantDish> RestaurantDishes { get; set; } = new List<RestaurantDish>();
    }
}
