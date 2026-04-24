using LifeQuestAPI.Application.DTOs;
using MediatR;

namespace LifeQuestAPI.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommandResponse
{
    public bool Success { get; set; }
    public string Intent { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string Response { get; set; } = string.Empty;
    public string Sentiment { get; set; } = string.Empty;
    public float SentimentConfidence { get; set; }
    public int UrgencyLevel { get; set; } = 0;
    public List<string> Suggestions { get; set; } = new();
    public List<ChatActionDto> Actions { get; set; } = new();
    public List<MiniTaskDto>? SuggestedTasks { get; set; }
    public bool ShouldShowMoodSelector { get; set; } = false;

    // Eski uyumluluk
    public Guid? SuggestedTaskId { get; set; }
    public string? SuggestedTaskTitle { get; set; }
    public int? SuggestedTaskXp { get; set; }
    public string? SuggestedTaskDifficulty { get; set; }
}
