using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Application.DTOs;

/// <summary>SP-02: Chat için kullanıcı bağlam objesi — Python AI servisine gönderilir.</summary>
public sealed record ChatContextDto
{
    public string UserName { get; init; } = string.Empty;
    public string Personality { get; init; } = "friendly"; // friendly | motivator | coach
    public List<TaskContextDto> TodayTasks { get; init; } = new();
    public int CompletedToday { get; init; }
    public int CurrentStreak { get; init; }
    public int TotalXP { get; init; }
    public string? TodayMood { get; init; }      // "Energetic", "Tired", vb.
    public int AvailableMinutes { get; init; } = 30;
}

public sealed record TaskContextDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int? EstimatedMinutes { get; init; }
}
