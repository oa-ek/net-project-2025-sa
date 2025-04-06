using System.ComponentModel.DataAnnotations;

namespace LocalFood.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
