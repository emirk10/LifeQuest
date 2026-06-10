namespace LifeQuestAPI.Application.DTOs;


public sealed record ChatActionDto
{
    public string Type { get; init; } = string.Empty;   
    public string Label { get; init; } = string.Empty;
    public Guid? TaskId { get; init; }
    public Dictionary<string, string>? Payload { get; init; }
}


public sealed record ChatActionResultDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ActionType { get; init; }
    public Guid? AffectedTaskId { get; init; }
}
