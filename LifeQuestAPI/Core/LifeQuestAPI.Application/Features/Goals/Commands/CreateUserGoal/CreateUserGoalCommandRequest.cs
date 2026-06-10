using System;
using LifeQuestAPI.Domain.Enums;
using MediatR;

namespace LifeQuestAPI.Application.Features.Goals.Commands.CreateUserGoal;

public class CreateUserGoalCommandRequest : IRequest<CreateUserGoalCommandResponse>
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GoalType Type { get; set; }
    public int TargetValue { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int BonusXP { get; set; }
    
    
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Why { get; set; }
    public string? ProgressUnit { get; set; }
    public int DifficultyLevel { get; set; } = 2;
    public bool AutoTrackByCategory { get; set; }
    public bool AutoTrackByXP { get; set; }
    public bool ReminderEnabled { get; set; } = true;
    public string? ReminderFrequency { get; set; }
}

public class CreateUserGoalCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? GoalId { get; set; }
}
