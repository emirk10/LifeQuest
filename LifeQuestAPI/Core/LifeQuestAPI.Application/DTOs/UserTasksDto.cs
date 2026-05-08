namespace LifeQuestAPI.Application.DTOs;

public sealed record UserTaskDto
{
    public Guid Id { get; set; }
    public Guid AppTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Xp { get; set; }
    public int BaseXP { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime AssignedAt { get; set; }
    public string? CategoryName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? PostponedTo { get; set; }
}