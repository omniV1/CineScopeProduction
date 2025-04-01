using CineScope.Shared.Auth;
using CineScope.Shared.DTOs;

namespace CineScope.Shared.Models
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

    public class RecaptchaResponse
    {
        public bool Success { get; set; }
        public string[] ErrorCodes { get; set; }
        public string ChallengeTs { get; set; }
        public string Hostname { get; set; }
    }
}