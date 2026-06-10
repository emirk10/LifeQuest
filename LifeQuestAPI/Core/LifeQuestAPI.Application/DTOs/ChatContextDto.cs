using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Application.DTOs;


public sealed record ChatContextDto
{
    public string UserName { get; init; } = string.Empty;
    public string Personality { get; init; } = "friendly"; 
    public List<TaskContextDto> TodayTasks { get; init; } = new();
    public int CompletedToday { get; init; }
    public int CurrentStreak { get; init; }
    public int TotalXP { get; init; }
    public string? TodayMood { get; init; }      
    public int AvailableMinutes { get; init; } = 30;

    
    public double GeneralLevel { get; init; } = 1.0;
    public string? MostSkippedCategory { get; init; }
    public string? PreferredMorningTime { get; init; }
    public string? DifficultyPreference { get; init; }
    public List<string> Strengths { get; init; } = new();
    public List<string> Weaknesses { get; init; } = new();
    public string? MotivationFactor { get; init; }
    public string? LifestyleType { get; init; }
    public string? PrimaryFocusArea { get; init; }
}

public sealed record TaskContextDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int? EstimatedMinutes { get; init; }
}
