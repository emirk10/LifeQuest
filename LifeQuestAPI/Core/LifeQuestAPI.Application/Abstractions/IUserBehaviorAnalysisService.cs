using LifeQuestAPI.Application.DTOs;

namespace LifeQuestAPI.Application.Abstractions;


public interface IUserBehaviorAnalysisService
{
    
    Task AnalyzeAndUpdateUserBehaviorAsync(Guid userId, CancellationToken ct = default);

    
    Task<UserBehaviorSummaryDto> GetBehaviorSummaryAsync(Guid userId, CancellationToken ct = default);
}
