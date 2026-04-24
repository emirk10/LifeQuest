using LifeQuestAPI.Application.DTOs;

namespace LifeQuestAPI.Application.Abstractions.AI;

/// <summary>
/// AI Chatbot servisi — Python mikro servisine mesaj + context gönderir ve zenginleştirilmiş cevap alır.
/// </summary>
public interface IAIChatService
{
    /// <summary>
    /// SP-01 uyumlu: Sadece mesaj ile temel chat (eski çağrılar için korunuyor).
    /// </summary>
    Task<ChatBotResponse> ProcessMessageAsync(Guid userId, string message);

    /// <summary>
    /// SP-02: Kullanıcı bağlamıyla zenginleştirilmiş chat — context-aware yanıt + actions.
    /// </summary>
    Task<ChatResponseDto> ProcessMessageWithContextAsync(Guid userId, string message, ChatContextDto context);
}
