// ViewModels/RestaurantCreateViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LocalFood.ViewModels
{
    public class RestaurantCreateViewModel
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Address { get; set; }
        
        public string Description { get; set; }
        
        // тут зберігаємо завантажений файл
        public IFormFile? UploadImage { get; set; }
    }
}
