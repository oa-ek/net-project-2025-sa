using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace LocalFood.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string FullAddress { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
