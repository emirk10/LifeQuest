using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LifeQuestAPI.Application.Abstractions.AI;
using LifeQuestAPI.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.Infrastructure.Services.AI;

/// <summary>
/// Python AI mikro servisine HTTP isteği atarak chatbot işlevselliği sağlar.
/// SP-02: Context-aware yanıt + action desteği eklendi.
/// </summary>
public class AIChatService : IAIChatService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIChatService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public AIChatService(HttpClient httpClient, ILogger<AIChatService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // SP-01 uyumluluk — eski çağrılar için
    public async Task<ChatBotResponse> ProcessMessageAsync(Guid userId, string message)
    {
        try
        {
            var request = new AIChatRequest { UserId = userId.ToString(), Message = message };
            var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AIChatApiResponse>();
            if (result == null) return GetFallbackResponse();

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI chat hatası. UserId: {UserId}", userId);
            return GetFallbackResponse();
        }
    }

    // SP-02: Context-aware yanıt
    public async Task<ChatResponseDto> ProcessMessageWithContextAsync(Guid userId, string message, ChatContextDto context)
    {
        try
        {
            var request = new AIChatContextRequest
            {
                UserId = userId.ToString(),
                Message = message,
                Context = new AIChatContextPayload
                {
                    UserName = context.UserName,
                    Personality = context.Personality,
                    TodayTasks = context.TodayTasks.Select(t => new AIChatTaskContext
                    {
                        Id = t.Id.ToString(),
                        Title = t.Title,
                        Status = t.Status,
                        Category = t.Category,
                        EstimatedMinutes = t.EstimatedMinutes
                    }).ToList(),
                    CompletedToday = context.CompletedToday,
                    CurrentStreak = context.CurrentStreak,
                    TotalXp = context.TotalXP,
                    TodayMood = context.TodayMood,
                    AvailableMinutes = context.AvailableMinutes
                }
            };

            var response = await _httpClient.PostAsJsonAsync("/api/chat", request, _jsonOptions);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AIChatContextApiResponse>(_jsonOptions);
            if (result == null) return GetFallbackContextResponse();

            return new ChatResponseDto
            {
                Intent = result.Intent,
                IntentConfidence = result.IntentConfidence,
                Sentiment = result.Sentiment,
                SentimentConfidence = result.SentimentConfidence,
                UrgencyLevel = result.UrgencyLevel,
                Response = result.Response,
                Suggestions = result.Suggestions ?? new(),
                Actions = (result.Actions ?? new()).Select(a => new ChatActionDto
                {
                    Type = a.Type,
                    Label = a.Label,
                    TaskId = a.TaskId.HasValue ? a.TaskId : null,
                    Payload = a.Payload
                }).ToList(),
                SuggestedTasks = result.SuggestedTasks?.Select(t => new MiniTaskDto
                {
                    TaskId = Guid.TryParse(t.TaskId, out var tid) ? tid : Guid.Empty,
                    Title = t.Title,
                    CategoryName = t.CategoryName,
                    EstimatedMinutes = t.EstimatedMinutes,
                    BaseXP = t.BaseXp,
                    Difficulty = t.Difficulty,
                    WhyThisTask = t.WhyThisTask
                }).ToList(),
                ShouldShowMoodSelector = result.ShouldShowMoodSelector
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI context chat hatası. UserId: {UserId}", userId);
            return GetFallbackContextResponse();
        }
    }

    private static ChatBotResponse GetFallbackResponse() => new()
    {
        Intent = "unknown",
        Confidence = 0,
        Response = "Şu anda AI asistanına ulaşamıyorum. Lütfen daha sonra tekrar dene! 🔄",
        Sentiment = "neutral",
        SentimentConfidence = 0,
        Suggestions = new() { "Görevlerim", "Profilim", "Yardım" }
    };

    private static ChatResponseDto GetFallbackContextResponse() => new()
    {
        Intent = "unknown",
        IntentConfidence = 0,
        Sentiment = "neutral",
        SentimentConfidence = 0,
        UrgencyLevel = 0,
        Response = "Şu anda AI asistanına ulaşamıyorum. Lütfen daha sonra tekrar dene! 🔄",
        Suggestions = new() { "Görevlerim", "Profilim", "Yardım" },
        Actions = new()
    };

    // ─── Request/Response iç modelleri ───────────────────────────────────────

    private class AIChatRequest
    {
        [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;
        [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    }

    private class AIChatContextRequest
    {
        [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;
        [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
        [JsonPropertyName("context")] public AIChatContextPayload? Context { get; set; }
    }

    private class AIChatContextPayload
    {
        [JsonPropertyName("user_name")] public string UserName { get; set; } = string.Empty;
        [JsonPropertyName("personality")] public string Personality { get; set; } = "friendly";
        [JsonPropertyName("today_tasks")] public List<AIChatTaskContext> TodayTasks { get; set; } = new();
        [JsonPropertyName("completed_today")] public int CompletedToday { get; set; }
        [JsonPropertyName("current_streak")] public int CurrentStreak { get; set; }
        [JsonPropertyName("total_xp")] public int TotalXp { get; set; }
        [JsonPropertyName("today_mood")] public string? TodayMood { get; set; }
        [JsonPropertyName("available_minutes")] public int AvailableMinutes { get; set; }
    }

    private class AIChatTaskContext
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
        [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
        [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
        [JsonPropertyName("estimated_minutes")] public int? EstimatedMinutes { get; set; }
    }

    private class AIChatApiResponse
    {
        [JsonPropertyName("intent")] public string Intent { get; set; } = string.Empty;
        [JsonPropertyName("confidence")] public float Confidence { get; set; }
        [JsonPropertyName("response")] public string Response { get; set; } = string.Empty;
        [JsonPropertyName("sentiment")] public string Sentiment { get; set; } = string.Empty;
        [JsonPropertyName("sentiment_confidence")] public float SentimentConfidence { get; set; }
        [JsonPropertyName("suggestions")] public List<string> Suggestions { get; set; } = new();
    }

    private class AIChatContextApiResponse
    {
        [JsonPropertyName("intent")] public string Intent { get; set; } = string.Empty;
        [JsonPropertyName("intent_confidence")] public float IntentConfidence { get; set; }
        [JsonPropertyName("sentiment")] public string Sentiment { get; set; } = string.Empty;
        [JsonPropertyName("sentiment_confidence")] public float SentimentConfidence { get; set; }
        [JsonPropertyName("urgency_level")] public int UrgencyLevel { get; set; }
        [JsonPropertyName("response")] public string Response { get; set; } = string.Empty;
        [JsonPropertyName("suggestions")] public List<string>? Suggestions { get; set; }
        [JsonPropertyName("actions")] public List<AIChatActionItem>? Actions { get; set; }
        [JsonPropertyName("suggested_tasks")] public List<AIChatMiniTask>? SuggestedTasks { get; set; }
        [JsonPropertyName("should_show_mood_selector")] public bool ShouldShowMoodSelector { get; set; }
    }

    private class AIChatActionItem
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
        [JsonPropertyName("task_id")] public Guid? TaskId { get; set; }
        [JsonPropertyName("payload")] public Dictionary<string, string>? Payload { get; set; }
    }

    private class AIChatMiniTask
    {
        [JsonPropertyName("task_id")] public string TaskId { get; set; } = string.Empty;
        [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
        [JsonPropertyName("category_name")] public string CategoryName { get; set; } = string.Empty;
        [JsonPropertyName("estimated_minutes")] public int? EstimatedMinutes { get; set; }
        [JsonPropertyName("base_xp")] public int BaseXp { get; set; }
        [JsonPropertyName("difficulty")] public string Difficulty { get; set; } = string.Empty;
        [JsonPropertyName("why_this_task")] public string? WhyThisTask { get; set; }
    }
}
