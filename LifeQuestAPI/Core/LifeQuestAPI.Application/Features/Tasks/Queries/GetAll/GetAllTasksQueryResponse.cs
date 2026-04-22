namespace LifeQuestAPI.Application.Features.Tasks.Queries.GetAll;

public sealed record GetAllTasksQueryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int BaseXP { get; set; }
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public bool IsDaily { get; set; }
    public bool IsActive { get; set; }
}