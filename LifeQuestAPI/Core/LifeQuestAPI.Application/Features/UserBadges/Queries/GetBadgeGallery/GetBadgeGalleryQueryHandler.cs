using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserBadges.Queries.GetBadgeGallery;


public sealed class GetBadgeGalleryQueryHandler : IRequestHandler<GetBadgeGalleryQueryRequest, GetBadgeGalleryQueryResponse>
{
    private readonly IBadgeReadRepository _badgeReadRepository;
    private readonly IUserBadgeReadRepository _userBadgeReadRepository;

    public GetBadgeGalleryQueryHandler(
        IBadgeReadRepository badgeReadRepository,
        IUserBadgeReadRepository userBadgeReadRepository)
    {
        _badgeReadRepository = badgeReadRepository;
        _userBadgeReadRepository = userBadgeReadRepository;
    }

    public async Task<GetBadgeGalleryQueryResponse> Handle(GetBadgeGalleryQueryRequest request, CancellationToken cancellationToken)
    {
        var allBadges = await _badgeReadRepository.GetAll(tracking: false)
            .OrderBy(b => b.RequiredValue)
            .ToListAsync(cancellationToken);

        var userEarnedBadges = await _userBadgeReadRepository
            .GetWhere(ub => ub.AppUserId == request.UserId, tracking: false)
            .Select(ub => new { ub.BadgeId, ub.EarnedAt })
            .ToListAsync(cancellationToken);

        var badgeDtos = allBadges.Select(badge =>
        {
            var earnedRecord = userEarnedBadges.FirstOrDefault(ub => ub.BadgeId == badge.Id);
            return new BadgeGalleryDto
            {
                Id = badge.Id,
                Name = badge.Name,
                Description = badge.Description,
                IconPath = badge.IconPath,
                RequiredLevel = badge.RequiredValue,
                IsEarned = earnedRecord != null,
                EarnedAt = earnedRecord?.EarnedAt
            };
        }).ToList();

        return new GetBadgeGalleryQueryResponse
        {
            Badges = badgeDtos
        };
    }
}