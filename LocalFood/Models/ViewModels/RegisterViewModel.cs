using System.ComponentModel.DataAnnotations;

namespace LocalFood.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Поле Email є обов'язковим.")]
        [EmailAddress(ErrorMessage = "Некоректний Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле пароль є обов'язковим.")]
        [StringLength(100, ErrorMessage = "{0} має бути не менше {2} символів.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердіть пароль")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають.")]
        public string ConfirmPassword { get; set; }
    }

}
