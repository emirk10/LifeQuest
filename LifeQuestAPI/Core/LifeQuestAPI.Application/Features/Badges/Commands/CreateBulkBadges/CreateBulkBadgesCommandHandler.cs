using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using MediatR;

namespace LifeQuestAPI.Application.Features.Badges.Commands.CreateBulkBadges;

public sealed class CreateBulkBadgesCommandHandler : IRequestHandler<CreateBulkBadgesCommandRequest, CreateBulkBadgesCommandResponse>
{
    private readonly IBadgeWriteRepository _badgeWriteRepository;

    public CreateBulkBadgesCommandHandler(IBadgeWriteRepository badgeWriteRepository)
    {
        _badgeWriteRepository = badgeWriteRepository;
    }

    public async Task<CreateBulkBadgesCommandResponse> Handle(CreateBulkBadgesCommandRequest request, CancellationToken cancellationToken)
    {
        var badgesToAdd = new List<Badge>();

        foreach (var dto in request.Badges)
        {
            if (string.IsNullOrEmpty(dto.Name))
                throw new Exception("HATA: Rozet listesindeki öğelerden birinin adı boş!");

            badgesToAdd.Add(new Badge
            {
                Name = dto.Name,
                Description = dto.Description,
                IconPath = string.IsNullOrEmpty(dto.IconPath) ? "default_badge.png" : dto.IconPath,
                RequiredValue = dto.RequiredLevel,
                Type = dto.CategoryId.HasValue && dto.CategoryId != Guid.Empty ? Domain.Enums.BadgeType.CategoryMaster : Domain.Enums.BadgeType.Level,
                CategoryId = dto.CategoryId == Guid.Empty ? null : dto.CategoryId,
            });
        }

        await _badgeWriteRepository.AddRangeAsync(badgesToAdd);
        await _badgeWriteRepository.SaveAsync();

        return new CreateBulkBadgesCommandResponse
        {
            IsSuccess = true,
            Message = $"{badgesToAdd.Count} adet rozet başarıyla eklendi.",
            AddedCount = badgesToAdd.Count
        };
    }
}