using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Abstractions.Social;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Infrastructure.Services.Social;

public sealed class ChallengeService : IChallengeService
{
    private readonly IChallengeReadRepository _challengeReadRepository;
    private readonly IChallengeWriteRepository _challengeWriteRepository;
    private readonly IGamificationService _gamificationService;
    private readonly IAppNotificationService _notificationService;

    public ChallengeService(
        IChallengeReadRepository challengeReadRepository,
        IChallengeWriteRepository challengeWriteRepository,
        IGamificationService gamificationService,
        IAppNotificationService notificationService)
    {
        _challengeReadRepository = challengeReadRepository;
        _challengeWriteRepository = challengeWriteRepository;
        _gamificationService = gamificationService;
        _notificationService = notificationService;
    }

    public async Task UpdateChallengeProgressAsync(Guid userId, Guid? categoryId)
    {
        var now = DateTime.UtcNow;

        // SP-05: Status enum ile aktif mücadeleleri getir
        var activeChallenges = await _challengeReadRepository.Table
            .Where(c => c.Status == ChallengeStatus.Active &&
                        c.StartDate <= now &&
                        c.EndDate >= now &&
                        (c.ChallengerId == userId || c.OpponentId == userId))
            .ToListAsync();

        if (!activeChallenges.Any()) return;

        foreach (var challenge in activeChallenges)
        {
            // CategoryFocus: Sadece hedef kategoride tamamlanan görevler sayılır
            if (challenge.ChallengeType == ChallengeType.CategoryFocus)
            {
                var targetCat = challenge.TargetCategoryId ?? challenge.CategoryId;
                if (targetCat.HasValue && categoryId.HasValue && targetCat != categoryId)
                    continue;
            }

            bool isWinnerDetermined = false;
            bool isChallenger = challenge.ChallengerId == userId;

            if (isChallenger)
                challenge.ChallengerProgress++;
            else
                challenge.OpponentProgress++;

            // TargetValue varsa — kazanan kontrolü
            if (challenge.TargetValue.HasValue)
            {
                int myProgress = isChallenger ? challenge.ChallengerProgress : challenge.OpponentProgress;
                if (myProgress >= challenge.TargetValue.Value)
                {
                    challenge.WinnerId = userId;
                    challenge.Status = ChallengeStatus.Completed;
                    isWinnerDetermined = true;
                }
            }

            if (isWinnerDetermined)
            {
                await _gamificationService.AddExperienceAsync(userId, challenge.WinnerXPBonus);

                var loserId = isChallenger ? challenge.OpponentId : challenge.ChallengerId;
                await _notificationService.SendNotificationAsync(
                    userId, "Mücadele Kazandın! 🏆",
                    $"'{challenge.Title}' mücadelesini kazandın! +{challenge.WinnerXPBonus} XP",
                    "ChallengeCompleted");
                await _notificationService.SendNotificationAsync(
                    loserId, "Mücadele Sona Erdi",
                    $"'{challenge.Title}' mücadelesinde rakibin seni geçti. Bir dahaki sefere!",
                    "ChallengeCompleted");
            }
            else
            {
                // Canlı ilerleme bildirimi (her 25% de bir)
                int myProg = isChallenger ? challenge.ChallengerProgress : challenge.OpponentProgress;
                int target = challenge.TargetValue ?? 100;
                int pct = (int)((double)myProg / target * 100);
                if (pct > 0 && pct % 25 == 0)
                {
                    var opponentId = isChallenger ? challenge.OpponentId : challenge.ChallengerId;
                    await _notificationService.SendNotificationAsync(
                        opponentId, "Rakibin İlerliyor! ⚠️",
                        $"'{challenge.Title}' mücadelesinde rakibin %{pct} tamamladı!",
                        "ChallengeProgressUpdated");
                }
            }
        }

        await _challengeWriteRepository.SaveAsync();
    }
}

