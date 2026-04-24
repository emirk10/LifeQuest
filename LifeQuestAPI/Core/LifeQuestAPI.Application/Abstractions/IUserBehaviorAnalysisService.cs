using LifeQuestAPI.Application.DTOs;

namespace LifeQuestAPI.Application.Abstractions;

/// <summary>SP-03: Kullanıcı davranış analizi servisi.</summary>
public interface IUserBehaviorAnalysisService
{
    /// <summary>Son 30 günün davranışını analiz edip AppUser tablolarını günceller.</summary>
    Task AnalyzeAndUpdateUserBehaviorAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Önceden hesaplanmış veya anlık hesaplanmış davranış özeti döndürür.</summary>
    Task<UserBehaviorSummaryDto> GetBehaviorSummaryAsync(Guid userId, CancellationToken ct = default);
}
