using MediatR;

namespace LifeQuestAPI.Application.Features.Social.Commands.GiveKudos;

public class GiveKudosCommandRequest : IRequest<GiveKudosCommandResponse>
{
    public Guid SenderId { get; set; }
    public Guid UserTaskId { get; set; }
}
