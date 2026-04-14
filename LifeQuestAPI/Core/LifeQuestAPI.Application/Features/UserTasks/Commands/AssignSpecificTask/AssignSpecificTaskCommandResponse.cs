using System;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.AssignSpecificTask;

public class AssignSpecificTaskCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? AssignedUserTaskId { get; set; }
}
