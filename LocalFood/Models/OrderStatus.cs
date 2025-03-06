using System.ComponentModel.DataAnnotations;

namespace LocalFood.Models
{
    public class OrderStatus
    {
        [Key]
        public int StatusId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
