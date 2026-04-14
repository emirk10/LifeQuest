using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LifeQuestAPI.Application.Abstractions.AI;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.Infrastructure.Services.AI;

/// <summary>
/// Python AI mikro servisine HTTP isteği atarak görüntü doğrulama yapar.
/// </summary>
public class ImageVerificationService : IImageVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageVerificationService> _logger;

    public ImageVerificationService(HttpClient httpClient, ILogger<ImageVerificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ImageVerificationResult> VerifyAsync(string imageBase64, string expectedCategory)
    {
        try
        {
            var request = new ImageVerifyApiRequest
            {
                UserId = "system",
                TaskId = "verification",
                TaskCategory = expectedCategory,
                ImageBase64 = imageBase64
            };

            var response = await _httpClient.PostAsJsonAsync("/api/verify-image", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ImageVerifyApiResponse>();

            if (result == null)
            {
                _logger.LogWarning("AI servisinden boş görüntü doğrulama cevabı döndü.");
                return GetFallbackResult(expectedCategory);
            }

            return new ImageVerificationResult
            {
                IsValid = result.IsValid,
                Confidence = result.Confidence,
                DetectedLabels = result.DetectedLabels?.Select(l => new DetectedLabel
                {
                    Label = l.Label,
                    Confidence = l.Confidence
                }).ToList() ?? new(),
                ExpectedCategory = result.ExpectedCategory,
                Message = result.Message
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI görüntü doğrulama servisine bağlanılamadı.");
            return GetFallbackResult(expectedCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görüntü doğrulama işleminde beklenmeyen hata.");
            return GetFallbackResult(expectedCategory);
        }
    }

    private static ImageVerificationResult GetFallbackResult(string expectedCategory)
    {
        // AI servisi çalışmıyorsa, admin incelemesine gönder (reject etme)
        return new ImageVerificationResult
        {
            IsValid = true, // Fallback: kabul et ama düşük güvenle
            Confidence = 0.1f,
            DetectedLabels = new(),
            ExpectedCategory = expectedCategory,
            Message = "⚠️ AI doğrulama servisi şu anda kullanılamıyor. Fotoğraf kabul edildi ancak sonradan incelenecek."
        };
    }

    private class ImageVerifyApiRequest
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("task_id")]
        public string TaskId { get; set; } = string.Empty;

        [JsonPropertyName("task_category")]
        public string TaskCategory { get; set; } = string.Empty;

        [JsonPropertyName("image_base64")]
        public string ImageBase64 { get; set; } = string.Empty;
    }

    private class ImageVerifyApiResponse
    {
        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        [JsonPropertyName("detected_labels")]
        public List<DetectedLabelDto>? DetectedLabels { get; set; }

        [JsonPropertyName("expected_category")]
        public string ExpectedCategory { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    private class DetectedLabelDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }
    }
}
