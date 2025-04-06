using System.ComponentModel.DataAnnotations;

namespace LocalFood.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "ПІБ обов’язкове")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Телефон обов’язковий")]
        [Phone]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Адреса доставки обов’язкова")]
        public string Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
