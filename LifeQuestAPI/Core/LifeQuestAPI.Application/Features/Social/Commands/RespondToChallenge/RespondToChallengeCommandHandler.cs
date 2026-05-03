using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Social.Commands.RespondToChallenge;

public class RespondToChallengeCommandRequest : IRequest<RespondToChallengeCommandResponse>
{
    public Guid UserId { get; set; }
    public Guid ChallengeId { get; set; }
    public bool Accept { get; set; }
    public string? Message { get; set; }
}

public class RespondToChallengeCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ChallengeStatus NewStatus { get; set; }
}

public sealed class RespondToChallengeCommandHandler
    : IRequestHandler<RespondToChallengeCommandRequest, RespondToChallengeCommandResponse>
{
    private readonly IChallengeReadRepository _challengeReadRepository;
    private readonly IChallengeWriteRepository _challengeWriteRepository;
    private readonly IAppNotificationService _notificationService;

    public RespondToChallengeCommandHandler(
        IChallengeReadRepository challengeReadRepository,
        IChallengeWriteRepository challengeWriteRepository,
        IAppNotificationService notificationService)
    {
        _challengeReadRepository = challengeReadRepository;
        _challengeWriteRepository = challengeWriteRepository;
        _notificationService = notificationService;
    }

    public async Task<RespondToChallengeCommandResponse> Handle(
        RespondToChallengeCommandRequest request, CancellationToken cancellationToken)
    {
        var challenge = await _challengeReadRepository
            .GetWhere(c => c.Id == request.ChallengeId && c.OpponentId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Mücadele bulunamadı veya bu davete cevap verme yetkiniz yok.");

        if (challenge.Status != ChallengeStatus.Invited)
            throw new Exception("Bu davete artık cevap verilemez.");

        if (request.Accept)
        {
            challenge.Status = ChallengeStatus.Active;
            challenge.AcceptedAt = DateTime.UtcNow;
            challenge.StartDate = DateTime.UtcNow;
            challenge.EndDate = DateTime.UtcNow.Add(challenge.EndDate - challenge.StartDate);

            // Her iki kullanıcıya bildirim
            await _notificationService.SendNotificationAsync(
                challenge.ChallengerId, "Mücadele Başladı! ⚔️",
                $"'{challenge.Title}' mücadeleniz başladı!", "ChallengeAccepted");
            await _notificationService.SendNotificationAsync(
                challenge.OpponentId, "Mücadele Başladı! ⚔️",
                $"'{challenge.Title}' mücadelesini kabul ettin!", "ChallengeAccepted");
        }
        else
        {
            challenge.Status = ChallengeStatus.Declined;

            // Davet edene bildirim
            await _notificationService.SendNotificationAsync(
                challenge.ChallengerId, "Mücadele Reddedildi",
                $"'{challenge.Title}' mücadele davetin reddedildi.", "ChallengeDeclined");
        }

        _challengeWriteRepository.Update(challenge);
        await _challengeWriteRepository.SaveAsync();

        return new RespondToChallengeCommandResponse
        {
            Success = true,
            Message = request.Accept ? "Mücadele kabul edildi, başladı!" : "Mücadele reddedildi.",
            NewStatus = challenge.Status
        };
    }
}
