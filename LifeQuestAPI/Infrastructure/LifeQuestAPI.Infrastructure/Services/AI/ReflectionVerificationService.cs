using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LifeQuestAPI.Application.Abstractions.AI;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.Infrastructure.Services.AI;

/// <summary>
/// Python AI mikro servisine HTTP isteği atarak yansıma metni doğrulaması yapar.
/// </summary>
public class ReflectionVerificationService : IReflectionVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReflectionVerificationService> _logger;

    public ReflectionVerificationService(HttpClient httpClient, ILogger<ReflectionVerificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ReflectionVerificationResult> VerifyAsync(string text, int minWordCount, string? taskCategory = null)
    {
        try
        {
            var request = new ReflectionVerifyApiRequest
            {
                Text = text,
                MinWordCount = minWordCount,
                TaskCategory = taskCategory
            };

            var response = await _httpClient.PostAsJsonAsync("/api/verify-reflection", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ReflectionVerifyApiResponse>();

            if (result == null)
            {
                _logger.LogWarning("AI servisinden boş yansıma doğrulama cevabı döndü.");
                return GetFallbackResult(text, minWordCount);
            }

            return new ReflectionVerificationResult
            {
                IsValid = result.IsValid,
                WordCount = result.WordCount,
                QualityScore = result.QualityScore,
                RelevanceScore = result.RelevanceScore,
                SpamDetected = result.SpamDetected,
                Issues = result.Issues,
                Message = result.Message
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI yansıma doğrulama servisine bağlanılamadı.");
            return GetFallbackResult(text, minWordCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yansıma doğrulama işleminde beklenmeyen hata.");
            return GetFallbackResult(text, minWordCount);
        }
    }

    /// <summary>
    /// AI servisi çalışmıyorsa basit kural tabanlı doğrulama yap.
    /// </summary>
    private static ReflectionVerificationResult GetFallbackResult(string text, int minWordCount)
    {
        var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var isValid = wordCount >= minWordCount;

        return new ReflectionVerificationResult
        {
            IsValid = isValid,
            WordCount = wordCount,
            QualityScore = isValid ? 0.5f : 0.2f,
            RelevanceScore = 0.5f,
            SpamDetected = false,
            Issues = isValid ? new() : new List<string> { $"Minimum {minWordCount} kelime gerekli (mevcut: {wordCount})" },
            Message = isValid
                ? "✅ Yansıma metni kabul edildi (basit doğrulama)."
                : $"❌ Metin çok kısa: {wordCount}/{minWordCount} kelime."
        };
    }

    private class ReflectionVerifyApiRequest
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("min_word_count")]
        public int MinWordCount { get; set; }

        [JsonPropertyName("task_category")]
        public string? TaskCategory { get; set; }
    }

    private class ReflectionVerifyApiResponse
    {
        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("word_count")]
        public int WordCount { get; set; }

        [JsonPropertyName("quality_score")]
        public float QualityScore { get; set; }

        [JsonPropertyName("relevance_score")]
        public float RelevanceScore { get; set; }

        [JsonPropertyName("spam_detected")]
        public bool SpamDetected { get; set; }

        [JsonPropertyName("issues")]
        public List<string> Issues { get; set; } = new();

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
