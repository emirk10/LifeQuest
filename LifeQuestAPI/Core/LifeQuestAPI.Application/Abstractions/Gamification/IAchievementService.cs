using System;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Abstractions.Gamification;

public interface IAchievementService
{
    /// <summary>
    /// Kullanıcının yeni bir başarım kazanıp kazanmadığını kontrol eder. Kazanırsa, DB'ye kaydeder ve bildirim atar.
    /// </summary>
    Task CheckAchievementsAsync(Guid userId);
}
