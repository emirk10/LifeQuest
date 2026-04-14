namespace LifeQuestAPI.Application.Abstractions.AI;

/// <summary>
/// Görüntü doğrulama servisi — Fotoğrafı Python AI servisine göndererek kategoriye uygunluğunu doğrular.
/// </summary>
public interface IImageVerificationService
{
    /// <summary>
    /// Base64 kodlanmış görüntüyü beklenen kategori ile doğrular.
    /// </summary>
    Task<ImageVerificationResult> VerifyAsync(string imageBase64, string expectedCategory);
}
