using MediatR;

namespace LifeQuestAPI.Application.Features.UserTasks.Queries.GetSharedTaskByToken;

public class GetSharedTaskByTokenQueryRequest : IRequest<GetSharedTaskByTokenQueryResponse>
{
    public string Token { get; set; } = string.Empty;
}
