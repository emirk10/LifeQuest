using MediatR;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.CompleteUserTask;

public class CompleteUserTaskCommandRequest : IRequest<CompleteUserTaskCommandResponse>
{
    public Guid UserTaskId { get; set; }
    public Guid UserId { get; set; }
}
