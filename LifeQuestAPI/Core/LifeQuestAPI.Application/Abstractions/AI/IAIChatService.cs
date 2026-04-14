namespace LifeQuestAPI.Application.Abstractions.AI;

/// <summary>
/// AI Chatbot servisi — Python mikro servisine mesaj gönderir ve cevap alır.
/// </summary>
public interface IAIChatService
{
    /// <summary>
    /// Kullanıcı mesajını AI servisine gönderir, intent + sentiment analizi yapar ve cevap döner.
    /// </summary>
    Task<ChatBotResponse> ProcessMessageAsync(Guid userId, string message);
}
