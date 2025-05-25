using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LocalFood.ViewModels
{
    public class RestaurantEditViewModel
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string? ImagePath { get; set; } // для відображення поточного фото
        public IFormFile? UploadImage { get; set; } // нове фото
    }


}
