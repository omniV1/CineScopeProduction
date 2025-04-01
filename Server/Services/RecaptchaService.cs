using CineScope.Shared.Config;
using CineScope.Shared.Models;
using Microsoft.Extensions.Options;

public class RecaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly RecaptchaSettings _settings;
    private readonly ILogger<RecaptchaService> _logger;
    private readonly Dictionary<string, int> _rateLimiter = new();
    private DateTime _rateLimiterResetTime = DateTime.UtcNow;

    public RecaptchaService(
        HttpClient httpClient,
        IOptions<RecaptchaSettings> settings,
        ILogger<RecaptchaService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.RequestTimeoutSeconds);
    }

    public async Task<bool> VerifyAsync(string recaptchaResponse)
    {
        if (!_settings.Enabled)
        {
            _logger.LogWarning("reCAPTCHA verification bypassed - service disabled");
            return true;
        }

        if (string.IsNullOrEmpty(recaptchaResponse))
        {
            _logger.LogWarning("Empty reCAPTCHA response received");
            return false;
        }

        if (!CheckRateLimit())
        {
            _logger.LogWarning("reCAPTCHA rate limit exceeded");
            return false;
        }

        for (int attempt = 1; attempt <= _settings.MaxRetries; attempt++)
        {
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", "6Ld8oAYrAAAAAMGmyPZcWAKsHFMU8KA2J1aPOcaP"),
                    new KeyValuePair<string, string>("response", recaptchaResponse)
                });

                var response = await _httpClient.PostAsync(_settings.VerifyUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("reCAPTCHA verification failed with status code: {StatusCode}",
                        response.StatusCode);

                    if (attempt < _settings.MaxRetries)
                    {
                        await Task.Delay(_settings.RetryDelayMilliseconds);
                        continue;
                    }

                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();

                if (result?.Success != true)
                {
                    _logger.LogWarning("reCAPTCHA verification failed. Error codes: {ErrorCodes}",
                        string.Join(", ", result?.ErrorCodes ?? Array.Empty<string>()));
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "reCAPTCHA verification attempt {Attempt} failed", attempt);

                if (attempt < _settings.MaxRetries)
                {
                    await Task.Delay(_settings.RetryDelayMilliseconds);
                    continue;
                }
            }
        }

        return false;
    }

    private bool CheckRateLimit()
    {
        var now = DateTime.UtcNow;
        var clientIp = "default"; // In a real app, get this from the request

        // Reset rate limiter every minute
        if ((now - _rateLimiterResetTime).TotalMinutes >= 1)
        {
            _rateLimiter.Clear();
            _rateLimiterResetTime = now;
        }

        if (!_rateLimiter.ContainsKey(clientIp))
        {
            _rateLimiter[clientIp] = 1;
            return true;
        }

        if (_rateLimiter[clientIp] >= _settings.RateLimitPerMinute)
        {
            return false;
        }

        _rateLimiter[clientIp]++;
        return true;
    }
}
