using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Badges.Commands.UpdateBadge;

public sealed record UpdateBadgeCommandRequest : IRequest<UpdateBadgeCommandResponse>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public BadgeType Type { get; set; }
    public int RequiredValue { get; set; }
    public bool IsSecret { get; set; }
    public string BadgeColor { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
}

public sealed record UpdateBadgeCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class UpdateBadgeCommandHandler : IRequestHandler<UpdateBadgeCommandRequest, UpdateBadgeCommandResponse>
{
    private readonly IBadgeWriteRepository _badgeWriteRepository;
    private readonly IBadgeReadRepository _badgeReadRepository;

    public UpdateBadgeCommandHandler(IBadgeWriteRepository badgeWriteRepository, IBadgeReadRepository badgeReadRepository)
    {
        _badgeWriteRepository = badgeWriteRepository;
        _badgeReadRepository = badgeReadRepository;
    }

    public async Task<UpdateBadgeCommandResponse> Handle(UpdateBadgeCommandRequest request, CancellationToken cancellationToken)
    {
        var badge = await _badgeReadRepository.GetByIdAsync(request.Id.ToString(), tracking: true);
        if (badge == null)
        {
            return new UpdateBadgeCommandResponse { IsSuccess = false, Message = "Rozet bulunamadı." };
        }

        badge.Name = request.Name;
        badge.Description = request.Description;
        badge.IconPath = request.IconPath;
        badge.Type = request.Type;
        badge.RequiredValue = request.RequiredValue;
        badge.IsSecret = request.IsSecret;
        badge.BadgeColor = request.BadgeColor;
        badge.Rarity = request.Rarity;
        badge.CategoryId = request.CategoryId;

        _badgeWriteRepository.Update(badge);
        await _badgeWriteRepository.SaveAsync();

        return new UpdateBadgeCommandResponse { IsSuccess = true, Message = "Rozet başarıyla güncellendi." };
    }
}
