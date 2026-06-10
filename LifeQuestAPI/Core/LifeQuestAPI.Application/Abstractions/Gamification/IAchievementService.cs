using System;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Abstractions.Gamification;

public interface IAchievementService
{
    
    
    
    Task CheckAchievementsAsync(Guid userId);
}
