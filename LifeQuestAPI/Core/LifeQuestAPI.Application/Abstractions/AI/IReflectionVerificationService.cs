namespace LifeQuestAPI.Application.Abstractions.AI;

/// <summary>
/// Yansıma metni doğrulama servisi — Metin kalitesini, ilgililiğini ve spam durumunu Python AI servisi ile kontrol eder.
/// </summary>
public interface IReflectionVerificationService
{
    /// <summary>
    /// Yansıma metnini minimum kelime sayısı ve görev kategorisi ile doğrular.
    /// </summary>
    Task<ReflectionVerificationResult> VerifyAsync(string text, int minWordCount, string? taskCategory = null);
}
