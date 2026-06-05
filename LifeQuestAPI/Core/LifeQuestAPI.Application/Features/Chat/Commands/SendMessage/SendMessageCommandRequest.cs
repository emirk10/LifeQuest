using MediatR;

namespace LifeQuestAPI.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommandRequest : IRequest<SendMessageCommandResponse>
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool SaveToHistory { get; set; } = true;
}
