using LifeQuestAPI.Application.DTOs;

namespace LifeQuestAPI.Application.Abstractions.AI;




public interface IAIChatService
{
    
    
    
    Task<ChatBotResponse> ProcessMessageAsync(Guid userId, string message);

    
    
    
    Task<ChatResponseDto> ProcessMessageWithContextAsync(Guid userId, string message, ChatContextDto context);
}
