using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Badges.Queries.GetAll;

public sealed record GetAllBadgesQueryRequest : IRequest<List<GetAllBadgesQueryResponse>>;

public sealed record GetAllBadgesQueryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int RequiredValue { get; set; }
    public bool IsSecret { get; set; }
    public string BadgeColor { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

public sealed class GetAllBadgesQueryHandler : IRequestHandler<GetAllBadgesQueryRequest, List<GetAllBadgesQueryResponse>>
{
    private readonly IBadgeReadRepository _badgeReadRepository;

    public GetAllBadgesQueryHandler(IBadgeReadRepository badgeReadRepository)
    {
        _badgeReadRepository = badgeReadRepository;
    }

    public async Task<List<GetAllBadgesQueryResponse>> Handle(GetAllBadgesQueryRequest request, CancellationToken cancellationToken)
    {
        var badges = await _badgeReadRepository
            .GetAll(tracking: false)
            .Include(b => b.Category)
            .ToListAsync(cancellationToken);

        return badges.Select(b => new GetAllBadgesQueryResponse
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description,
            IconPath = b.IconPath,
            Type = b.Type.ToString(),
            RequiredValue = b.RequiredValue,
            IsSecret = b.IsSecret,
            BadgeColor = b.BadgeColor,
            Rarity = b.Rarity,
            CategoryId = b.CategoryId,
            CategoryName = b.Category != null ? b.Category.Name : null
        }).ToList();
    }
}
