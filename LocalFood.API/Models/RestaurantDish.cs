using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalFood.API.Models
{
    public class RestaurantDish
    {
        [Key]
        public int Id { get; set; }

        [Required, ForeignKey("Restaurant")]
        public int RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; }

        [Required, ForeignKey("Dish")]
        public int DishId { get; set; }
        public virtual Dish Dish { get; set; }
    }
}
