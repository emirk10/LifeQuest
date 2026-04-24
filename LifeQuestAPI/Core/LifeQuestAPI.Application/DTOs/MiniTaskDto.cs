namespace LifeQuestAPI.Application.DTOs;

/// <summary>SP-02: Chatbot içinde görev önerisi için kompakt kart DTO'su.</summary>
public sealed record MiniTaskDto
{
    public Guid TaskId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public int? EstimatedMinutes { get; init; }
    public int BaseXP { get; init; }
    public string Difficulty { get; init; } = string.Empty;
    public string? WhyThisTask { get; init; }
}
