namespace LifeQuestAPI.Application.DTOs;


public sealed record ChatResponseDto
{
    public string Intent { get; init; } = string.Empty;
    public float IntentConfidence { get; init; }
    public string Sentiment { get; init; } = string.Empty;
    public float SentimentConfidence { get; init; }
    public int UrgencyLevel { get; init; } = 0;  
    public string Response { get; init; } = string.Empty;
    public List<string> Suggestions { get; init; } = new();
    public List<ChatActionDto> Actions { get; init; } = new();
    public List<MiniTaskDto>? SuggestedTasks { get; init; }
    public bool ShouldShowMoodSelector { get; init; } = false;
    public string? FlowRedirect { get; init; }

    
    public Guid? SuggestedTaskId { get; init; }
    public string? SuggestedTaskTitle { get; init; }
    public int? SuggestedTaskXp { get; init; }
    public string? SuggestedTaskDifficulty { get; init; }
}
