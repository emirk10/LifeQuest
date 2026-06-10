using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Social.Commands.SendChallengeInvite;

public class SendChallengeInviteCommandRequest : IRequest<SendChallengeInviteCommandResponse>
{
    public Guid ChallengerId { get; set; }
    public Guid OpponentId { get; set; }
    public ChallengeType ChallengeType { get; set; } = ChallengeType.TaskCount;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int DurationDays { get; set; } = 7;
    public int? TargetValue { get; set; }
    public Guid? TargetCategoryId { get; set; }
    public Guid? TargetAppTaskId { get; set; }
    public string? ChallengerMessage { get; set; }
    public int WinnerXPBonus { get; set; } = 100;
    public bool IsPublic { get; set; } = false;
}

public class SendChallengeInviteCommandResponse
{
    public bool Success { get; set; }
    public Guid ChallengeId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class SendChallengeInviteCommandHandler
    : IRequestHandler<SendChallengeInviteCommandRequest, SendChallengeInviteCommandResponse>
{
    private readonly IChallengeWriteRepository _challengeWriteRepository;
    private readonly IChallengeReadRepository _challengeReadRepository;
    private readonly IChallengeParticipantWriteRepository _participantWriteRepository;
    private readonly IFriendshipReadRepository _friendshipReadRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppNotificationService _notificationService;

    public SendChallengeInviteCommandHandler(
        IChallengeWriteRepository challengeWriteRepository,
        IChallengeReadRepository challengeReadRepository,
        IChallengeParticipantWriteRepository participantWriteRepository,
        IFriendshipReadRepository friendshipReadRepository,
        IAppUserReadRepository userReadRepository,
        IAppNotificationService notificationService)
    {
        _challengeWriteRepository = challengeWriteRepository;
        _challengeReadRepository = challengeReadRepository;
        _participantWriteRepository = participantWriteRepository;
        _friendshipReadRepository = friendshipReadRepository;
        _userReadRepository = userReadRepository;
        _notificationService = notificationService;
    }

    public async Task<SendChallengeInviteCommandResponse> Handle(
        SendChallengeInviteCommandRequest request, CancellationToken cancellationToken)
    {
        
        var areFriends = await _friendshipReadRepository.GetWhere(
            f => ((f.RequesterId == request.ChallengerId && f.AddresseeId == request.OpponentId) ||
                  (f.RequesterId == request.OpponentId && f.AddresseeId == request.ChallengerId)) &&
                 f.Status == FriendshipStatus.Accepted,
            tracking: false)
            .AnyAsync(cancellationToken);

        if (!areFriends)
            throw new Exception("Mücadele daveti göndermek için karşı tarafın arkadaşın olması gerekir.");

        
        var hasActiveChallenge = await _challengeReadRepository.GetWhere(
            c => (c.ChallengerId == request.ChallengerId && c.OpponentId == request.OpponentId ||
                  c.ChallengerId == request.OpponentId && c.OpponentId == request.ChallengerId) &&
                 (c.Status == ChallengeStatus.Active || c.Status == ChallengeStatus.Invited),
            tracking: false)
            .AnyAsync(cancellationToken);

        if (hasActiveChallenge)
            throw new Exception("Bu kullanıcı ile zaten aktif bir mücadeleniz var.");

        
        var opponent = await _userReadRepository.GetByIdAsync(request.OpponentId.ToString())
            ?? throw new Exception("Rakip kullanıcı bulunamadı.");

        
        var title = request.Title ?? $"{request.ChallengeType} Mücadelesi";

        var challenge = new Challenge
        {
            ChallengerId = request.ChallengerId,
            OpponentId = request.OpponentId,
            ChallengeType = request.ChallengeType,
            Status = ChallengeStatus.Invited,
            Title = title,
            Description = request.Description,
            TargetCategoryId = request.TargetCategoryId,
            TargetAppTaskId = request.TargetAppTaskId,
            TargetValue = request.TargetValue,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(request.DurationDays),
            InvitedAt = DateTime.UtcNow,
            ChallengerMessage = request.ChallengerMessage,
            WinnerXPBonus = request.WinnerXPBonus,
            IsPublic = request.IsPublic
        };

        await _challengeWriteRepository.AddAsync(challenge);
        await _challengeWriteRepository.SaveAsync();

        
        await _participantWriteRepository.AddAsync(new ChallengeParticipant
        {
            ChallengeId = challenge.Id,
            UserId = request.ChallengerId,
            Role = 0, 
            JoinedAt = DateTime.UtcNow
        });
        await _participantWriteRepository.AddAsync(new ChallengeParticipant
        {
            ChallengeId = challenge.Id,
            UserId = request.OpponentId,
            Role = 1, 
            JoinedAt = DateTime.UtcNow
        });
        await _participantWriteRepository.SaveAsync();

        
        await _notificationService.SendNotificationAsync(
            request.OpponentId,
            "Mücadele Daveti! ⚔️",
            $"'{title}' mücadelesine davet edildin! {request.ChallengerMessage ?? ""}",
            "ChallengeInvitation");

        return new SendChallengeInviteCommandResponse
        {
            Success = true,
            ChallengeId = challenge.Id,
            Message = "Mücadele daveti gönderildi!"
        };
    }
}
