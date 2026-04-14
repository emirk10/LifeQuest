namespace LifeQuestAPI.Application.DTOs;

public sealed record UserTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Xp { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }

    // Dogrulama alanlari
    public int VerificationType { get; set; }
    public int MinReflectionWords { get; set; }
    public int MinTimerMinutes { get; set; }
    public string? VerificationHint { get; set; }
    public string? ExpectedImageCategory { get; set; }
}