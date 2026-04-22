using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Social;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeQuestAPI.Infrastructure.Services.Social;

public sealed class ChallengeService : IChallengeService
{
    private readonly IChallengeReadRepository _challengeReadRepository;
    private readonly IChallengeWriteRepository _challengeWriteRepository;
    private readonly IGamificationService _gamificationService;

    public ChallengeService(
        IChallengeReadRepository challengeReadRepository,
        IChallengeWriteRepository challengeWriteRepository,
        IGamificationService gamificationService)
    {
        _challengeReadRepository = challengeReadRepository;
        _challengeWriteRepository = challengeWriteRepository;
        _gamificationService = gamificationService;
    }

    public async Task UpdateChallengeProgressAsync(Guid userId, Guid? categoryId)
    {
        // Aktif olan, süresi geçmemiş ve kullanıcının dahil olduğu mücadeleleri getir
        var now = DateTime.UtcNow;
        var activeChallenges = await _challengeReadRepository.Table
            .Where(c => c.IsActive && 
                        c.StartDate <= now && 
                        c.EndDate >= now &&
                        (c.ChallengerId == userId || c.OpponentId == userId))
            .ToListAsync();

        if (!activeChallenges.Any()) return;

        foreach (var challenge in activeChallenges)
        {
            // Eğer mücadele kategori bazlıysa ve tamamlanan görev farklı kategorideyse atla
            if (challenge.CategoryId.HasValue && categoryId.HasValue && challenge.CategoryId != categoryId)
                continue;

            bool isWinnerDetermined = false;

            if (challenge.ChallengerId == userId)
            {
                challenge.ChallengerProgress++;
                if (challenge.ChallengerProgress >= challenge.TargetTaskCount)
                {
                    challenge.WinnerId = userId;
                    challenge.IsActive = false;
                    isWinnerDetermined = true;
                }
            }
            else if (challenge.OpponentId == userId)
            {
                challenge.OpponentProgress++;
                if (challenge.OpponentProgress >= challenge.TargetTaskCount)
                {
                    challenge.WinnerId = userId;
                    challenge.IsActive = false;
                    isWinnerDetermined = true;
                }
            }

            // Kazanan belirlendiyse ödül XP ver
            if (isWinnerDetermined)
            {
                await _gamificationService.AddExperienceAsync(userId, challenge.WinnerBonusXP);
            }
        }

        await _challengeWriteRepository.SaveAsync();
    }
}
