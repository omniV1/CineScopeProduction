namespace CineScope.Shared.Config
{
    public class RecaptchaSettings
    {
        public string SiteKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public double MinimumScore { get; set; } = 0.5;
        public string VerifyUrl { get; set; } = "https://www.google.com/recaptcha/api/siteverify";
        public int RequestTimeoutSeconds { get; set; } = 10;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayMilliseconds { get; set; } = 1000;
        public int RateLimitPerMinute { get; set; } = 100;
        public bool Enabled { get; set; } = true;
    }
}