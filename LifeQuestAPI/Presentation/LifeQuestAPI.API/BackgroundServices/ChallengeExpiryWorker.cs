using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.API.BackgroundServices;


public class ChallengeExpiryWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChallengeExpiryWorker> _logger;

    public ChallengeExpiryWorker(IServiceProvider serviceProvider, ILogger<ChallengeExpiryWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ChallengeExpiryWorker başlatıldı.");

        using PeriodicTimer timer = new(TimeSpan.FromHours(1));
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await ProcessExpiredChallengesAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChallengeExpiryWorker hata aldı.");
            }
        }
    }

    private async Task ProcessExpiredChallengesAsync(IServiceProvider services, CancellationToken ct)
    {
        var challengeReadRepo = services.GetRequiredService<IChallengeReadRepository>();
        var challengeWriteRepo = services.GetRequiredService<IChallengeWriteRepository>();
        var gamificationService = services.GetRequiredService<IGamificationService>();
        var notificationService = services.GetRequiredService<IAppNotificationService>();
        var now = DateTime.UtcNow;

        
        var expiredInvites = await challengeReadRepo.GetWhere(
            c => c.Status == ChallengeStatus.Invited &&
                 c.InvitedAt.AddHours(48) < now,
            tracking: true)
            .ToListAsync(ct);

        foreach (var c in expiredInvites)
        {
            c.Status = ChallengeStatus.Expired;
            challengeWriteRepo.Update(c);
        }

        
        var expiredActive = await challengeReadRepo.GetWhere(
            c => c.Status == ChallengeStatus.Active && c.EndDate < now,
            tracking: true)
            .ToListAsync(ct);

        foreach (var c in expiredActive)
        {
            c.Status = ChallengeStatus.Completed;

            
            Guid? winnerId = null;
            int challengerProg = c.ChallengerProgress;
            int opponentProg = c.OpponentProgress;

            if (challengerProg > opponentProg) winnerId = c.ChallengerId;
            else if (opponentProg > challengerProg) winnerId = c.OpponentId;
            

            c.WinnerId = winnerId;

            if (winnerId.HasValue)
            {
                await gamificationService.AddExperienceAsync(winnerId.Value, c.WinnerXPBonus);
                var loserId = winnerId.Value == c.ChallengerId ? c.OpponentId : c.ChallengerId;

                await notificationService.SendNotificationAsync(
                    winnerId.Value, "Mücadele Kazandın! 🏆",
                    $"'{c.Title}' mücadelesini kazandın! +{c.WinnerXPBonus} XP", "ChallengeCompleted");
                await notificationService.SendNotificationAsync(
                    loserId, "Mücadele Bitti",
                    $"'{c.Title}' mücadelesinde rakibin daha iyi oldu. Bir daha dene!", "ChallengeCompleted");
            }
            else
            {
                
                int halfBonus = c.WinnerXPBonus / 2;
                await gamificationService.AddExperienceAsync(c.ChallengerId, halfBonus);
                await gamificationService.AddExperienceAsync(c.OpponentId, halfBonus);

                await notificationService.SendNotificationAsync(
                    c.ChallengerId, "Mücadele Bitti — Beraberlik!",
                    $"'{c.Title}' mücadelesi berabere bitti! +{halfBonus} XP", "ChallengeCompleted");
                await notificationService.SendNotificationAsync(
                    c.OpponentId, "Mücadele Bitti — Beraberlik!",
                    $"'{c.Title}' mücadelesi berabere bitti! +{halfBonus} XP", "ChallengeCompleted");
            }

            challengeWriteRepo.Update(c);
        }

        await challengeWriteRepo.SaveAsync();
        _logger.LogInformation("ChallengeExpiryWorker: {Exp} süresi doldu, {Comp} tamamlandı.",
            expiredInvites.Count, expiredActive.Count);
    }
}
