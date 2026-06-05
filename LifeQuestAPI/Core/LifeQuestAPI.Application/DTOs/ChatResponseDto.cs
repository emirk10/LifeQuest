namespace LifeQuestAPI.Application.DTOs;

/// <summary>SP-02: Zenginleştirilmiş AI yanıt DTO'su — Python servisinden alınan full response.</summary>
public sealed record ChatResponseDto
{
    public string Intent { get; init; } = string.Empty;
    public float IntentConfidence { get; init; }
    public string Sentiment { get; init; } = string.Empty;
    public float SentimentConfidence { get; init; }
    public int UrgencyLevel { get; init; } = 0;  // 0=Normal, 1=Mild, 2=Moderate, 3=High
    public string Response { get; init; } = string.Empty;
    public List<string> Suggestions { get; init; } = new();
    public List<ChatActionDto> Actions { get; init; } = new();
    public List<MiniTaskDto>? SuggestedTasks { get; init; }
    public bool ShouldShowMoodSelector { get; init; } = false;
    public string? FlowRedirect { get; init; }

    // Geriye dönük uyumluluk (eski ChatBot.jsx ile)
    public Guid? SuggestedTaskId { get; init; }
    public string? SuggestedTaskTitle { get; init; }
    public int? SuggestedTaskXp { get; init; }
    public string? SuggestedTaskDifficulty { get; init; }
}
