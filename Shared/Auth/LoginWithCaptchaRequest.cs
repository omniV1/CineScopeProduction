using System.ComponentModel.DataAnnotations;

namespace CineScope.Shared.Auth
{
    public class LoginWithCaptchaRequest
    {
        [Required]
        public LoginRequest LoginRequest { get; set; }

        [Required]
        public string RecaptchaResponse { get; set; }
    }
} 