using LifeQuestAPI.Application.DTOs;

namespace LifeQuestAPI.Application.Features.UserTasks.Queries.GetUserTasks;

public class GetUserTasksQueryResponse
{
    public List<UserTaskDto> Tasks { get; set; } = new();
    public int TotalCount { get; set; }
}
