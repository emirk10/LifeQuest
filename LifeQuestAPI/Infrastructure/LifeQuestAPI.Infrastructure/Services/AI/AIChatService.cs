using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LifeQuestAPI.Application.Abstractions.AI;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.Infrastructure.Services.AI;

/// <summary>
/// Python AI mikro servisine HTTP isteği atarak chatbot işlevselliği sağlar.
/// </summary>
public class AIChatService : IAIChatService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIChatService> _logger;

    public AIChatService(HttpClient httpClient, ILogger<AIChatService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ChatBotResponse> ProcessMessageAsync(Guid userId, string message)
    {
        try
        {
            var request = new AIChatRequest
            {
                UserId = userId.ToString(),
                Message = message
            };

            var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AIChatApiResponse>();

            if (result == null)
            {
                _logger.LogWarning("AI servisten boş cevap döndü. UserId: {UserId}", userId);
                return GetFallbackResponse();
            }

            return new ChatBotResponse
            {
                Intent = result.Intent,
                Confidence = result.Confidence,
                Response = result.Response,
                Sentiment = result.Sentiment,
                SentimentConfidence = result.SentimentConfidence,
                Suggestions = result.Suggestions
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI servisine bağlanılamadı. UserId: {UserId}", userId);
            return GetFallbackResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI chat işleminde beklenmeyen hata. UserId: {UserId}", userId);
            return GetFallbackResponse();
        }
    }

    private static ChatBotResponse GetFallbackResponse()
    {
        return new ChatBotResponse
        {
            Intent = "unknown",
            Confidence = 0,
            Response = "Şu anda AI asistanına ulaşamıyorum. Lütfen daha sonra tekrar dene! 🔄",
            Sentiment = "neutral",
            SentimentConfidence = 0,
            Suggestions = new List<string> { "Görevlerim", "Profilim", "Yardım" }
        };
    }

    // Python servisine gönderilen istek modeli
    private class AIChatRequest
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    // Python servisinden dönen cevap modeli
    private class AIChatApiResponse
    {
        [JsonPropertyName("intent")]
        public string Intent { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("sentiment")]
        public string Sentiment { get; set; } = string.Empty;

        [JsonPropertyName("sentiment_confidence")]
        public float SentimentConfidence { get; set; }

        [JsonPropertyName("suggestions")]
        public List<string> Suggestions { get; set; } = new();
    }
}
