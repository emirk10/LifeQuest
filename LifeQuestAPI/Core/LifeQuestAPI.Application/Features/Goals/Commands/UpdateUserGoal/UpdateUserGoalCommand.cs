using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Goals.Commands.UpdateUserGoal;

public sealed record UpdateUserGoalCommandRequest : IRequest<UpdateUserGoalCommandResponse>
{
    public Guid GoalId { get; set; }
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
    public string ProgressUnit { get; set; } = "görev";
    public bool AutoTrackByCategory { get; set; }
    public bool AutoTrackByXP { get; set; }
    public bool ReminderEnabled { get; set; }
    public string ReminderFrequency { get; set; } = "weekly";
}

public sealed record UpdateUserGoalCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class UpdateUserGoalCommandHandler : IRequestHandler<UpdateUserGoalCommandRequest, UpdateUserGoalCommandResponse>
{
    private readonly IUserGoalWriteRepository _userGoalWriteRepository;
    private readonly IUserGoalReadRepository _userGoalReadRepository;

    public UpdateUserGoalCommandHandler(IUserGoalWriteRepository userGoalWriteRepository, IUserGoalReadRepository userGoalReadRepository)
    {
        _userGoalWriteRepository = userGoalWriteRepository;
        _userGoalReadRepository = userGoalReadRepository;
    }

    public async Task<UpdateUserGoalCommandResponse> Handle(UpdateUserGoalCommandRequest request, CancellationToken cancellationToken)
    {
        var goal = await _userGoalReadRepository.GetByIdAsync(request.GoalId.ToString(), tracking: true);
        if (goal == null || goal.AppUserId != request.UserId)
        {
            return new UpdateUserGoalCommandResponse { IsSuccess = false, Message = "Hedef bulunamadı." };
        }

        goal.Title = request.Title;
        goal.Description = request.Description;
        goal.Type = request.Type;
        goal.TargetValue = request.TargetValue;
        goal.CategoryId = request.CategoryId;
        goal.StartDate = request.StartDate;
        goal.EndDate = request.EndDate;
        goal.BonusXP = request.BonusXP;
        goal.Icon = request.Icon;
        goal.Color = request.Color;
        goal.Why = request.Why;
        goal.ProgressUnit = request.ProgressUnit;
        goal.AutoTrackByCategory = request.AutoTrackByCategory;
        goal.AutoTrackByXP = request.AutoTrackByXP;
        goal.ReminderEnabled = request.ReminderEnabled;
        goal.ReminderFrequency = request.ReminderFrequency;

        _userGoalWriteRepository.Update(goal);
        await _userGoalWriteRepository.SaveAsync();

        return new UpdateUserGoalCommandResponse { IsSuccess = true, Message = "Hedef başarıyla güncellendi." };
    }
}
