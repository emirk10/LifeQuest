using MediatR;
using System;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.AssignSpecificTask;

public class AssignSpecificTaskCommandRequest : IRequest<AssignSpecificTaskCommandResponse>
{
    public Guid UserId { get; set; }
    public Guid AppTaskId { get; set; }
}
