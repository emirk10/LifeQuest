using LifeQuestAPI.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Badges.Commands.DeleteBadge;

public sealed record DeleteBadgeCommandRequest : IRequest<DeleteBadgeCommandResponse>
{
    public Guid Id { get; set; }
}

public sealed record DeleteBadgeCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class DeleteBadgeCommandHandler : IRequestHandler<DeleteBadgeCommandRequest, DeleteBadgeCommandResponse>
{
    private readonly IBadgeWriteRepository _badgeWriteRepository;
    private readonly IBadgeReadRepository _badgeReadRepository;

    public DeleteBadgeCommandHandler(IBadgeWriteRepository badgeWriteRepository, IBadgeReadRepository badgeReadRepository)
    {
        _badgeWriteRepository = badgeWriteRepository;
        _badgeReadRepository = badgeReadRepository;
    }

    public async Task<DeleteBadgeCommandResponse> Handle(DeleteBadgeCommandRequest request, CancellationToken cancellationToken)
    {
        var badge = await _badgeReadRepository.GetByIdAsync(request.Id.ToString(), tracking: true);
        if (badge == null)
        {
            return new DeleteBadgeCommandResponse { IsSuccess = false, Message = "Rozet bulunamadı." };
        }

        _badgeWriteRepository.Remove(badge);
        await _badgeWriteRepository.SaveAsync();

        return new DeleteBadgeCommandResponse { IsSuccess = true, Message = "Rozet başarıyla silindi." };
    }
}
