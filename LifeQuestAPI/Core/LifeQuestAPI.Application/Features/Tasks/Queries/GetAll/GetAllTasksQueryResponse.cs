namespace LifeQuestAPI.Application.Features.Tasks.Queries.GetAll;

public sealed record GetAllTasksQueryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int BaseXP { get; set; }
    public int MinLevel { get; set; }

    public string VerificationType { get; set; } = string.Empty;
    public int MinReflectionWords { get; set; }
    public int MinTimerMinutes { get; set; }
    public string? VerificationHint { get; set; }
    public string? ExpectedImageCategory { get; set; }
}