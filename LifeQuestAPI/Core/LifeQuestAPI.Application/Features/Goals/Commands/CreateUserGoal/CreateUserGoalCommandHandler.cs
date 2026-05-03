using System;
using System.Threading;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using MediatR;

namespace LifeQuestAPI.Application.Features.Goals.Commands.CreateUserGoal;

public class CreateUserGoalCommandHandler : IRequestHandler<CreateUserGoalCommandRequest, CreateUserGoalCommandResponse>
{
    private readonly IUserGoalWriteRepository _goalWriteRepository;

    public CreateUserGoalCommandHandler(IUserGoalWriteRepository goalWriteRepository)
    {
        _goalWriteRepository = goalWriteRepository;
    }

    public async Task<CreateUserGoalCommandResponse> Handle(CreateUserGoalCommandRequest request, CancellationToken cancellationToken)
    {
        var goal = new UserGoal
        {
            Id = Guid.NewGuid(),
            AppUserId = request.UserId,
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            TargetValue = request.TargetValue,
            CurrentValue = 0,
            CategoryId = request.CategoryId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            BonusXP = request.BonusXP,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            // SP-05
            Icon = request.Icon,
            Color = request.Color,
            Why = request.Why,
            ProgressUnit = request.ProgressUnit ?? "görev",
            AutoTrackByCategory = request.AutoTrackByCategory,
            AutoTrackByXP = request.AutoTrackByXP,
            ReminderEnabled = request.ReminderEnabled,
            ReminderFrequency = request.ReminderFrequency ?? "daily",
            LinkedCategoryIds = request.CategoryId.HasValue ? new List<Guid> { request.CategoryId.Value } : null
        };

        await _goalWriteRepository.AddAsync(goal);
        await _goalWriteRepository.SaveAsync();

        return new CreateUserGoalCommandResponse
        {
            Success = true,
            Message = "Hedef başarıyla oluşturuldu.",
            GoalId = goal.Id
        };
    }
}
