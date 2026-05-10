namespace LifeQuestAPI.Application.Features.UserTasks.Queries.GetSharedTaskByToken;

public class GetSharedTaskByTokenQueryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TaskTitle { get; set; }
    public string? TaskDescription { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryIcon { get; set; }
    public string? UserName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ReflectionText { get; set; }
    public List<string>? Tags { get; set; }
}
