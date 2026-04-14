using MediatR;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.CompleteUserTask;

public class CompleteUserTaskCommandRequest : IRequest<CompleteUserTaskCommandResponse>
{
    public Guid UserTaskId { get; set; }
    public Guid UserId { get; set; }

    // Dogrulama verileri
    public string? PhotoBase64 { get; set; }
    public string? ReflectionText { get; set; }
    public int? TimerSeconds { get; set; }
    public int? QuizScore { get; set; }
    public int? QuizTotalQuestions { get; set; }
}
