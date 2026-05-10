namespace LifeQuestAPI.Application.Features.UserTasks.Commands.ShareUserTask;

public class ShareUserTaskCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ShareToken { get; set; }
    public string? ShareUrl { get; set; }
}
