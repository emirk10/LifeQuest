using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using MediatR;

namespace LifeQuestAPI.Application.Features.Badges.Commands.CreateBadge;

public sealed class CreateBadgeCommandHandler : IRequestHandler<CreateBadgeCommandRequest, CreateBadgeCommandResponse>
{
    private readonly IBadgeWriteRepository _badgeWriteRepository;

    public CreateBadgeCommandHandler(IBadgeWriteRepository badgeWriteRepository)
    {
        _badgeWriteRepository = badgeWriteRepository;
    }

    public async Task<CreateBadgeCommandResponse> Handle(CreateBadgeCommandRequest request, CancellationToken cancellationToken)
    {
        var newBadge = new Badge
        {
            Name = request.Name,
            Description = request.Description,
            IconPath = string.IsNullOrEmpty(request.IconPath) ? "default_badge.png" : request.IconPath,
            RequiredValue = request.RequiredLevel,
            Type = request.CategoryId.HasValue && request.CategoryId != Guid.Empty ? Domain.Enums.BadgeType.CategoryMaster : Domain.Enums.BadgeType.Level,
            CategoryId = request.CategoryId == Guid.Empty ? null : request.CategoryId,
        };

        await _badgeWriteRepository.AddAsync(newBadge);
        await _badgeWriteRepository.SaveAsync();

        return new CreateBadgeCommandResponse
        {
            IsSuccess = true,
            Message = $"'{newBadge.Name}' rozeti başarıyla oluşturuldu.",
            BadgeId = newBadge.Id
        };
    }
}
