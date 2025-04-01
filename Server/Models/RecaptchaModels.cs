using CineScope.Shared.Auth;
using CineScope.Shared.DTOs;

namespace CineScope.Server.Models
{
    public class RecaptchaRequest
    {
        public string RecaptchaResponse { get; set; }
    }

    public class RegisterWithCaptchaRequest
    {
        public RegisterRequest RegisterRequest { get; set; }
        public string RecaptchaResponse { get; set; }
    }

    public class ReviewWithCaptchaRequest
    {
        public ReviewDto Review { get; set; }
        public string RecaptchaResponse { get; set; }
    }
}