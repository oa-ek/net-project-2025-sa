using System.ComponentModel.DataAnnotations;

namespace LocalFood.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле Email є обов'язковим.")]
        [EmailAddress(ErrorMessage = "Некоректний Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле пароль є обов'язковим.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Запам'ятати мене?")]
        public bool RememberMe { get; set; }
    }
}
