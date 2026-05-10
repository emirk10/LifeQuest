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

    public async Task UpdateChallengeProgressAsync(Guid userId, Guid? categoryId, int earnedXp)
    {
        var now = DateTime.UtcNow;

        // Status enum ile aktif mücadeleleri getir
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

            // Puan artışı (XPRace ise XP kadar, değilse 1 birim)
            int increment = challenge.ChallengeType == ChallengeType.XPRace ? earnedXp : 1;

            if (isChallenger)
                challenge.ChallengerProgress += increment;
            else
                challenge.OpponentProgress += increment;

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
                var loserId = isChallenger ? challenge.OpponentId : challenge.ChallengerId;
                await _notificationService.SendNotificationAsync(
                    userId, "Mücadele Kazandın! 🏆",
                    $"'{challenge.Title}' mücadelesini kazandın!",
                    "ChallengeCompleted");
                await _notificationService.SendNotificationAsync(
                    loserId, "Mücadele Sona Erdi",
                    $"'{challenge.Title}' mücadelesinde rakibin seni geçti. Bir dahaki sefere!",
                    "ChallengeCompleted");
            }
            else
            {
                // Canlı ilerleme bildirimi (her 25% de bir veya XPRace için her güncellemede?)
                int myProg = isChallenger ? challenge.ChallengerProgress : challenge.OpponentProgress;
                int target = challenge.TargetValue ?? 100;
                int pct = (int)((double)myProg / target * 100);
                
                // XPRace ise her zaman bildirim gönderilebilir veya belli eşiklerde
                if (challenge.ChallengeType == ChallengeType.XPRace || (pct > 0 && pct % 25 == 0))
                {
                    var opponentId = isChallenger ? challenge.OpponentId : challenge.ChallengerId;
                    await _notificationService.SendNotificationAsync(
                        opponentId, "Rakibin İlerliyor! ⚠️",
                        $"'{challenge.Title}' mücadelesinde rakibin ilerleme kaydetti!",
                        "ChallengeProgressUpdated");
                }
            }
        }

        await _challengeWriteRepository.SaveAsync();
    }
}

