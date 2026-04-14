namespace LifeQuestAPI.Application.Features.UserTasks.Commands.CompleteUserTask;

public class CompleteUserTaskCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int EarnedXp { get; set; }
    public string? VerificationMessage { get; set; }
}
