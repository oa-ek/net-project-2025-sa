using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalFood.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [Required]
        public int DishId { get; set; }

        [ForeignKey("DishId")]
        public virtual Dish? Dish { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Кількість повинна бути не менше 1.")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
