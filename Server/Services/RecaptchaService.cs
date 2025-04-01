using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CineScope.Server.Services
{
    public class RecaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;
        private readonly ILogger<RecaptchaService> _logger;
        private readonly bool _isEnabled;

        public RecaptchaService(HttpClient httpClient, IConfiguration configuration, ILogger<RecaptchaService> logger)
        {
            _httpClient = httpClient;
            _secretKey = configuration["Recaptcha:SecretKey"];
            _logger = logger;
            _isEnabled = !string.IsNullOrEmpty(_secretKey);

            if (!_isEnabled)
            {
                _logger.LogWarning("reCAPTCHA is not configured. Verification will be bypassed.");
            }
        }

        public async Task<bool> VerifyAsync(string recaptchaResponse)
        {
            if (!_isEnabled)
            {
                _logger.LogWarning("reCAPTCHA verification bypassed due to missing configuration.");
                return true; // Skip verification if not configured
            }

            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                _logger.LogWarning("Empty reCAPTCHA response received.");
                return false;
            }

            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", _secretKey),
                    new KeyValuePair<string, string>("response", recaptchaResponse)
                });

                var response = await _httpClient.PostAsync(
                    "https://www.google.com/recaptcha/api/siteverify", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("reCAPTCHA verification failed with status code: {StatusCode}",
                        response.StatusCode);
                    return false;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<RecaptchaResponse>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Success != true)
                {
                    _logger.LogWarning("reCAPTCHA verification failed. Error codes: {ErrorCodes}",
                        string.Join(", ", result?.ErrorCodes ?? Array.Empty<string>()));
                }

                return result?.Success ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during reCAPTCHA verification");
                return false;
            }
        }

        private class RecaptchaResponse
        {
            public bool Success { get; set; }
            public DateTime? ChallengeTs { get; set; }
            public string Hostname { get; set; }
            public string[] ErrorCodes { get; set; }
        }
    }
}