using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LocalFood.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Ім’я обов’язкове")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Прізвище обов’язкове")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Телефон обов’язковий")]
        [Phone]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Адреса доставки обов’язкова")]
        public string Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public List<AddressViewModel> SavedAddresses { get; set; } = new List<AddressViewModel>();
        public int? SelectedAddressId { get; set; }

        // Для автозаповнення із профіля
         [BindNever]
        public string ProfileFirstName { get; set; }
         [BindNever]
        public string ProfileLastName { get; set; }
         [BindNever]
        public string ProfilePhone { get; set; }
    }

}
