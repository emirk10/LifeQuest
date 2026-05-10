using LifeQuestAPI.Application.DTOs.Common;
using MediatR;

namespace LifeQuestAPI.Application.Features.User.Queries.GetLeaderboard;

public sealed record GetLeaderboardQueryRequest(
    int PageNumber = 1, 
    int PageSize = 10, 
    string Filter = "global", 
    Guid? CurrentUserId = null,
    string TimeRange = "alltime"
) : IRequest<PaginatedList<GetLeaderboardQueryResponse>>;
