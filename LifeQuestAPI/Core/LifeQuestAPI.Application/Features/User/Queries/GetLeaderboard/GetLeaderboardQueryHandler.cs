using LifeQuestAPI.Application.Common.Extensions;
using LifeQuestAPI.Application.DTOs.Common;
using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.User.Queries.GetLeaderboard;

public sealed class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQueryRequest, PaginatedList<GetLeaderboardQueryResponse>>
{
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IFriendshipReadRepository _friendshipReadRepository;

    public GetLeaderboardQueryHandler(IAppUserReadRepository userReadRepository, IFriendshipReadRepository friendshipReadRepository)
    {
        _userReadRepository = userReadRepository;
        _friendshipReadRepository = friendshipReadRepository;
    }

    public async Task<PaginatedList<GetLeaderboardQueryResponse>> Handle(GetLeaderboardQueryRequest request, CancellationToken cancellationToken)
    {
        var userQuery = _userReadRepository.Table.AsNoTracking();

        if (request.Filter == "friends" && request.CurrentUserId.HasValue)
        {
            var friendIds = await _friendshipReadRepository.Table.AsNoTracking()
                .Where(f => (f.RequesterId == request.CurrentUserId || f.AddresseeId == request.CurrentUserId) && f.Status == LifeQuestAPI.Domain.Enums.FriendshipStatus.Accepted)
                .Select(f => f.RequesterId == request.CurrentUserId ? f.AddresseeId : f.RequesterId)
                .ToListAsync(cancellationToken);
            
            friendIds.Add(request.CurrentUserId.Value); // Kendisi de listede olsun

            userQuery = userQuery.Where(u => friendIds.Contains(u.Id));
        }

        var query = userQuery
                      .OrderByDescending(u => u.TotalXP)
                      .Select(user => new GetLeaderboardQueryResponse
                      {
                          Rank = 0,
                          UserName = user.UserName,
                          Name = user.Name,
                          Surname = user.Surname,
                          TotalXP = user.TotalXP,
                          GeneralLevel = user.GeneralLevel
                      });

        var paginatedResult = await query.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        
        // Fix ranks based on page
        int baseRank = (request.PageNumber - 1) * request.PageSize;
        for (int i = 0; i < paginatedResult.Items.Count; i++)
        {
            paginatedResult.Items[i].Rank = baseRank + i + 1;
        }

        return paginatedResult;
    }
}
