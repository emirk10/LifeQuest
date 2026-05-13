using MediatR;

namespace LifeQuestAPI.Application.Features.UserTasks.Queries.GetUserTasks;

public class GetUserTasksQueryRequest : IRequest<GetUserTasksQueryResponse>
{
    public Guid UserId { get; set; }
    public Guid? CategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
