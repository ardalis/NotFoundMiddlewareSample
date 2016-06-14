using System.ComponentModel.DataAnnotations;

namespace NotFoundMiddlewareSample.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
