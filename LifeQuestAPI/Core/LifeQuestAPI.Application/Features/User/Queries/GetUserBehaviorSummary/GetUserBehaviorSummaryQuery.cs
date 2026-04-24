using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.DTOs;
using MediatR;

namespace LifeQuestAPI.Application.Features.User.Queries.GetUserBehaviorSummary;

public class GetUserBehaviorSummaryQueryRequest : IRequest<UserBehaviorSummaryDto>
{
    public Guid UserId { get; set; }
}

public sealed class GetUserBehaviorSummaryQueryHandler
    : IRequestHandler<GetUserBehaviorSummaryQueryRequest, UserBehaviorSummaryDto>
{
    private readonly IUserBehaviorAnalysisService _behaviorService;

    public GetUserBehaviorSummaryQueryHandler(IUserBehaviorAnalysisService behaviorService)
    {
        _behaviorService = behaviorService;
    }

    public Task<UserBehaviorSummaryDto> Handle(
        GetUserBehaviorSummaryQueryRequest request, CancellationToken cancellationToken)
        => _behaviorService.GetBehaviorSummaryAsync(request.UserId, cancellationToken);
}
