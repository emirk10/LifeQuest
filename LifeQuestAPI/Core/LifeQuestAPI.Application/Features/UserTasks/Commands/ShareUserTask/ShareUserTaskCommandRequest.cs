using MediatR;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.ShareUserTask;

public class ShareUserTaskCommandRequest : IRequest<ShareUserTaskCommandResponse>
{
    public Guid UserTaskId { get; set; }
    public Guid UserId { get; set; }
}
